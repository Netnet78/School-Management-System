using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace SchoolManagement.Presentation.Features.Students.ViewModels
{
    public partial class AssignCandidateViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private readonly ICandidateService _candidateService;
        private readonly IStudentService _studentService;
        private readonly IPhotoFetchService _photoFetchService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IDispatcherService _dispatcherService;

        private CancellationTokenSource? _filterCts;
        private CancellationTokenSource? _photoDownloadCts;
        private Timer? _searchDebounceTimer;
        private const int SearchDebounceDelayMs = 500;

        private const int PageSize = 100;

        public AssignCandidateViewModel(
            ICandidateService candidateService,
            IStudentService studentService,
            IPhotoFetchService photoFetchService,
            IMessageService messageService,
            INavigationService navigationService,
            IDispatcherService dispatcherService)
        {
            _candidateService = candidateService;
            _studentService = studentService;
            _photoFetchService = photoFetchService;
            _messageService = messageService;
            _navigationService = navigationService;
            _dispatcherService = dispatcherService;
        }

        [ObservableProperty]
        private bool _dataLoading;

        [ObservableProperty]
        private int _currentPage = 1;

        async partial void OnCurrentPageChanged(int value)
        {
            if (value > MaximumPages)
            {
                CurrentPage = MaximumPages;
                return;
            }
            if (value <= 0)
            {
                CurrentPage = 1;
                return;
            }
        }

        [ObservableProperty]
        private int _maximumPages = 1;

        private ObservableCollection<Candidate> _allCandidates = [];

        [ObservableProperty]
        private ICollectionView? _candidatesView;

        [ObservableProperty]
        private Candidate? _selectedCandidate;

        [ObservableProperty]
        private bool _isLoadingPhoto;

        [ObservableProperty]
        private FileObject? _selectedCandidatePhoto;

        [ObservableProperty]
        private bool _validPhotoPath;

        [ObservableProperty]
        private string _searchText = string.Empty;

        async partial void OnSearchTextChanged(string value)
        {
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(
                async (_) =>
                {
                    CurrentPage = 1;
                    await LoadCandidatesAsync();
                },
                null,
                SearchDebounceDelayMs,
                Timeout.Infinite);
        }

        [ObservableProperty]
        private Gender? _selectedGender;

        [ObservableProperty]
        private DateTime? _fromDate;

        [ObservableProperty]
        private DateTime? _toDate;

        [ObservableProperty]
        private string? _sortBy = "Id";

        [ObservableProperty]
        private OrderDirection _orderType = OrderDirection.Descending;

        public IEnumerable<OrderDirection> OrderTypeOptions { get; } =
            Enum.GetValues<OrderDirection>();

        [ObservableProperty]
        private StudentDataStateFilterOptions _dataStateFilter = StudentDataStateFilterOptions.All;

        public IEnumerable<object> DataStateFilterItems { get; } =
            Enum.GetValues<StudentDataStateFilterOptions>()
                .Select(s => new { Value = s, Description = $"{s.GetDescription()}" });

        async partial void OnSelectedCandidateChanged(Candidate? value)
        {
            _photoDownloadCts?.Cancel();

            SelectedCandidatePhoto = null;
            if (value == null)
            {
                ValidPhotoPath = false;
                return;
            }

            CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            _photoDownloadCts = cts;

            CancellationToken token = cts.Token;

            try
            {
                IsLoadingPhoto = true;
                if (!string.IsNullOrWhiteSpace(value.PhotoKey))
                {
                    SelectedCandidatePhoto = (await _photoFetchService.GetStudentPhoto(value.PhotoKey)).Value;
                    ValidPhotoPath = File.Exists(SelectedCandidatePhoto?.FilePath);
                }
                else
                {
                    ValidPhotoPath = false;
                }
            }
            catch (OperationCanceledException)
            {
                ValidPhotoPath = false;
            }
            finally
            {
                IsLoadingPhoto = false;
            }
        }

        [RelayCommand]
        private async Task LoadCandidatesAsync()
        {
            DataLoading = true;
            try
            {
                List<FilterCondition<Candidate>> filters = BuildFilters();
                ReturnResponse<int> countResponse = await _candidateService.GetAllCountAsync(filters, DataStateFilter);
                if (countResponse.Status == Status.Failed)
                {
                    await _dispatcherService.InvokeAsync(() =>
                    {
                        _messageService.Show(countResponse.Message, "Error", MessageButton.OK, MessageIcon.Error);
                    });
                    return;
                }

                ReturnResponse<IEnumerable<Candidate>> response = await _candidateService.GetAllAsync(
                    CurrentPage, PageSize, filters, DataStateFilter, SortBy, OrderType);
                if (response.Status == Status.Failed || response.Value == null)
                {
                    await _dispatcherService.InvokeAsync(() =>
                    {
                        _messageService.Show(response.Message, "Error", MessageButton.OK, MessageIcon.Error);
                    });
                    return;
                }

                _allCandidates = [..response.Value];
                MaximumPages = (int)Math.Ceiling((double)countResponse.Value / PageSize);

                CandidatesView = CollectionViewSource.GetDefaultView(_allCandidates);
                CandidatesView.Refresh();
            }
            finally
            {
                DataLoading = false;
            }
        }

        private List<FilterCondition<Candidate>> BuildFilters()
        {
            return
            [
                new(c => c.Gender, FilterOperator.Equals, SelectedGender),
                new(c => c.CreatedAt, FilterOperator.GreaterThanOrEqual,
                    FromDate.HasValue ? FromDate.Value.ToUniversalTime().Date : null),
                new(c => c.CreatedAt, FilterOperator.LessThan,
                    ToDate.HasValue ? ToDate.Value.ToUniversalTime().Date.AddDays(1) : null),
                new(c => c.FullName, FilterOperator.Contains, SearchText)
            ];
        }

        [RelayCommand]
        private async Task AssignSelectedCandidateAsync()
        {
            Candidate? candidate = SelectedCandidate;
            if (candidate == null)
            {
                _messageService.Show("សូមជ្រើសរើសបេក្ខជនណាម្នាក់ជាមុនសិន!", "ឈប់សិន!", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (candidate.Student != null)
            {
                MessageResult result = MessageResult.None;
                await _dispatcherService.InvokeAsync(() =>
                {
                    result = _messageService.Show(
                        $"បេក្ខជនឈ្មោះ {candidate.FullName} ត្រូវបានដាក់ជាសិស្សរួចហើយ។",
                        "មានរួចហើយ!", MessageButton.OK, MessageIcon.Information);
                });
            }

            IsLoadingPhoto = true;
            try
            {
                Student student = new() { CandidateId = candidate.Id };
                ReturnResponse insertResponse = await _studentService.InsertAsync(student);

                if (insertResponse.Status == Status.Failed)
                {
                    await _dispatcherService.InvokeAsync(() =>
                    {
                        _messageService.Show(insertResponse.Message ?? "Failed to assign candidate as student.", "Error", MessageButton.OK, MessageIcon.Error);
                    });
                    return;
                }

                await _navigationService.NavigateAsync<AssignStudentClassViewModel>(new AssignStudentClassParams { Student = student });
            }
            finally
            {
                IsLoadingPhoto = false;
            }
        }

        [RelayCommand]
        private async Task ChangePageIndex(string value)
        {
            int steps = int.Parse(value);
            int newPage = steps + CurrentPage;
            CurrentPage = newPage;
            await LoadCandidatesAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            CurrentPage = 1;
            await LoadCandidatesAsync();
        }

        [RelayCommand]
        private async Task ClearFiltersAsync()
        {
            SelectedGender = null;
            FromDate = null;
            ToDate = null;
            SortBy = "Id";
            OrderType = OrderDirection.Descending;
            SearchText = string.Empty;
            await LoadCandidatesAsync();
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await _navigationService.NavigateAsync<AddStudentOptionViewModel>();
        }

        public async Task LoadAsync()
        {
            await LoadCandidatesAsync();
        }
    }
}
