using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace CandidateManagement.ViewModels
{
    public partial class StudentViewModel : ObservableObject, IAsyncLoadable, IViewModel
    {
        private readonly ICandidateService _candidateService;
        private readonly IPhotoDeleteService _photoDeleteService;
        private readonly IPhotoFetchService _photoFetchService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IDispatcherService _dispatcherService;

        // Debouncing and async filtering
        private CancellationTokenSource? _filterCts;
        private CancellationTokenSource? _photoDownloadCts;
        private Timer? _searchDebounceTimer;
        private const int SearchDebounceDelayMs = 500;

        public StudentViewModel(
            ICandidateService candidateService,
            IPhotoDeleteService photoDeleteService,
            IPhotoFetchService photoFetchService,
            IMessageService messageService,
            INavigationService navigationService,
            IDispatcherService dispatcherService)
        {
            _candidateService = candidateService;
            _photoDeleteService = photoDeleteService;
            _photoFetchService = photoFetchService;
            _messageService = messageService;
            _navigationService = navigationService;
            _dispatcherService = dispatcherService;
        }

        // Students per page limit
        private readonly int _studentsPerPage = 100;

        // Loading states
        [ObservableProperty]
        private bool _dataLoading = false;

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

        // ទិន្នន័យសិស្សទាំងអស់ដែលមាននៅក្នុង database
        private ObservableCollection<Candidate> _allStudents = [];

        // ទិន្នន័យសិស្សដែលត្រូវបានបង្ហាញនៅលើ UI
        [ObservableProperty]
        private ICollectionView? _studentsView;

        // សិស្សដែលកំពុងជ្រើសរើស
        [ObservableProperty]
        private Candidate? _selectedStudent;

        [ObservableProperty]
        private bool _showFilterDialog = false;

        [ObservableProperty]
        private bool _toggleLatinNames = false;

        [ObservableProperty]
        private Gender? _selectedGender;

        public IEnumerable<GenderOption> GenderOptions { get; } =
            Enum.GetValues<Gender>().Select(g =>
            {
                return new GenderOption()
                {
                    KhmerName = g.GetDescription(),
                    Name = g.ToString(),
                    Value = g
                };
            });

        [ObservableProperty]
        private DateTime? _fromDate;

        [ObservableProperty]
        private DateTime? _toDate;

        [ObservableProperty]
        public string? _sortBy;

        [ObservableProperty]
        private OrderDirection _orderType = OrderDirection.Descending;

        public IEnumerable<OrderDirection> OrderTypeOptions { get; } =
            Enum.GetValues<OrderDirection>();

        async partial void OnSelectedStudentChanged(Candidate? value)
        {
            _photoDownloadCts?.Cancel();

            SelectedStudentPhoto = null;
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
                    SelectedStudentPhoto = (await _photoFetchService.GetStudentPhoto(value.PhotoKey)).Value?.FilePath;
                    ValidPhotoPath = File.Exists(SelectedStudentPhoto);
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

        [ObservableProperty]
        private bool _isLoadingPhoto;

        [ObservableProperty]
        private string? _selectedStudentPhoto;

        [ObservableProperty]
        private bool _validPhotoPath;

        // ច្រោះទិន្នន័យតាម​ស្ថានភាពទិន្នន័យ
        [ObservableProperty]
        private StudentDataStateFilterOptions _dataStateFilter = StudentDataStateFilterOptions.All;

        async partial void OnDataStateFilterChanged(StudentDataStateFilterOptions value)
        {
            // Only refresh the view; preserve SelectedStudent if it still matches the filter
            await RefreshStudentsViewAsync();
        }

        // ទិន្នន័យសម្រាប់ Combo box ​ច្រោះទិន្នន័យតាម​ស្ថានភាពទិន្នន័យ
        public IEnumerable<object> DataStateFilterItems
        { get; } = Enum.GetValues<StudentDataStateFilterOptions>()
            .Select(s => new { Value = s, Description = $"{s.GetDescription()}" });

        // ច្រោះទិន្នន័យតាម​ការស្វែងរក
        [ObservableProperty]
        private string _studentSearch = string.Empty;

        async partial void OnStudentSearchChanged(string value)
        {
            // Debounce the search: wait 300ms after user stops typing before filtering
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(
                async (_) =>
                {
                    CurrentPage = 1;
                    await RefreshStudentsViewAsync();
                },
                null,
                SearchDebounceDelayMs,
                Timeout.Infinite);
        }

        [RelayCommand]
        private void ToggleFilterDialog()
        {
            ShowFilterDialog = !ShowFilterDialog;
        }

        [RelayCommand]
        private async Task ClearFilters()
        {
            SelectedGender = null;
            FromDate = null;
            ToDate = null;
            SortBy = string.Empty;
            OrderType = OrderDirection.Descending;
            await RefreshStudentsViewAsync();
        }

        [RelayCommand]
        private async Task SetFilters()
        {
            await RefreshStudentsViewAsync();
            ShowFilterDialog = false;
        }

        [RelayCommand]
        private async Task ChangePageIndex(string value)
        {
            int steps = int.Parse(value);

            int newPage = steps + CurrentPage;

            CurrentPage = newPage;

            await LoadStudentsAsync();
        }

        // Relay Commands
        [RelayCommand]
        private async Task LoadStudentsAsync()
        {
            DataLoading = true;
            try
            {
                List<FilterCondition<Candidate>> filters = BuildFilters();
                ReturnResponse<int> countResponse = await _candidateService.GetAllCountAsync(
                    filters, DataStateFilter);
                if (countResponse.Status == Status.Failed)
                {
                    await _dispatcherService.InvokeAsync(() =>
                    {
                        _messageService.Show(countResponse.Message, "Error", button: MessageButton.OK, MessageIcon.Error);
                    });
                    return;
                }

                ReturnResponse<IEnumerable<Candidate>> studentsResponse = await _candidateService.GetAllAsync(
                    CurrentPage, _studentsPerPage,
                    filters, DataStateFilter, SortBy, (OrderDirection)OrderType);
                IEnumerable<Candidate>? students = studentsResponse.Value;
                if (studentsResponse.Status == Status.Failed || students == null)
                {
                    await _dispatcherService.InvokeAsync(() =>
                    {
                        _messageService.Show(studentsResponse.Message, "Error", button: MessageButton.OK, MessageIcon.Error);
                    });
                    return;
                }

                _allStudents = [..students];
                MaximumPages = (int)Math.Ceiling((double)countResponse.Value / _studentsPerPage);

                StudentsView = CollectionViewSource.GetDefaultView(_allStudents);
                StudentsView.Refresh();
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
                new(c => c.FullName, FilterOperator.Contains, StudentSearch)
            ];
        }

        [RelayCommand]
        private async Task EditStudentAsync()
        {
            if (SelectedStudent == null) return;

            await _navigationService.NavigateAsync<EditStudentViewModel>
            (
                new EditStudentParams()
                {
                    Candidate = SelectedStudent
                }
            );
        }

        [RelayCommand]
        private async Task DeleteStudentAsync()
        {
            Candidate? student = SelectedStudent;

            if (student == null) return;

            MessageResult result = MessageResult.None;
            await _dispatcherService.InvokeAsync(() =>
            {
                result = _messageService.Show($"តើអ្នកប្រាកដថាលុបសិស្សឈ្មោះ {student.FullName} ដែរឬទេ?", "Confirm Delete", MessageButton.YesNo, MessageIcon.Exclamation);
            });

            if (result != MessageResult.Yes) return;

            ReturnResponse deleteResponse = await _candidateService.DeleteCandidateAsync(student.Id);

            if (deleteResponse.Status == Status.Failed || deleteResponse.Status == Status.Rejected)
            {
                await _dispatcherService.InvokeAsync(() =>
                {
                    _messageService.Show($"{deleteResponse.Message}", "ហាក... ស្អីគេ?", icon: MessageIcon.Error);
                });
                return;
            }

            if (!string.IsNullOrWhiteSpace(student.PhotoKey))
            {
                ReturnResponse deletePhotoResponse = await _photoDeleteService.DeleteStudentPhoto(student);

                if (deletePhotoResponse.Status == Status.Failed || deletePhotoResponse.Status == Status.Rejected)
                {
                    await _dispatcherService.InvokeAsync(() =>
                    {
                        _messageService.Show($"{deletePhotoResponse.Message}", "ហាក... ស្អីគេ?", icon: MessageIcon.Error);
                    });
                    return;
                }
            }

            await LoadStudentsAsync();
        }

        // Initial async load
        public async Task LoadAsync()
        {
            await LoadStudentsAsync();
        }

        // Helper functions
        /// <summary>
        /// Refreshes the students view with optimized filtering.
        /// Uses async cancellation to avoid processing stale filter requests.
        /// </summary>
        async private Task RefreshStudentsViewAsync()
        {
            if (StudentsView == null) return;

            // Cancel any previous filter operation
            _filterCts?.Cancel();
            _filterCts = new CancellationTokenSource();

            await LoadStudentsAsync();
            await _dispatcherService.InvokeAsync(StudentsView.Refresh);
        }
    }
}
