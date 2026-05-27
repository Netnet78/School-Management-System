using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Features.Attendances.Models;
using SchoolManagement.Core.Shared.Extensions;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Attendances.ViewModels
{
    public partial class AttendanceViewModel : ObservableObject, IViewModel, IAsyncLoadable
    {
        private const int DefaultPageSize = 10;

        private readonly IAttendanceService _attendanceService;
        private readonly IClassService _classService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private ObservableCollection<Attendance> _allAttendances = [];
        private CancellationTokenSource? _cts;

        [ObservableProperty]
        private ObservableCollection<Attendance> _attendances = [];

        [ObservableProperty]
        private ObservableCollection<Class> _classes = [];

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canManageAttendances;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int? _classFilterId;

        [ObservableProperty]
        private AttendanceStatus? _statusFilter;

        [ObservableProperty]
        private DateTime? _dateFromFilter;

        [ObservableProperty]
        private DateTime? _dateToFilter;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _maxPage = 1;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _currentPageTotalCount;

        [ObservableProperty]
        private string _pageCount = string.Empty;

public IEnumerable<object> StatusOptions { get; } = Enum.GetValues<AttendanceStatus>()
    .Select(s => new { Value = (AttendanceStatus?)s, Description = s.GetDescription() })
    .Prepend(new { Value = (AttendanceStatus?)null, Description = "ទាំងអស់" });

        public AttendanceViewModel(
            IAttendanceService attendanceService,
            IClassService classService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService)
        {
            _attendanceService = attendanceService;
            _classService = classService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
        }

        public async Task LoadAsync()
        {
            ReturnResponse permission = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageAttendances);
            if (permission.Status != Status.Success)
            {
                CanManageAttendances = false;
                _messageService.Show(permission.Message ?? "You do not have permission to manage attendances.", "Access denied", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            CanManageAttendances = true;

            await LoadClassesAsync();
            await LoadAttendancesAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadAttendancesAsync();
        }

        [RelayCommand]
        private async Task AddAttendanceAsync()
        {
            if (!CanManageAttendances)
            {
                return;
            }

            await _navigationService.NavigateAsync<SelectClassViewModel>();
        }

        [RelayCommand]
        private async Task EditAttendanceAsync(Attendance? attendance)
        {
            if (!CanManageAttendances || attendance == null)
            {
                return;
            }

            await _navigationService.NavigateAsync<EditAttendanceViewModel>(new EditAttendanceParams { Attendance = attendance });
        }

        [RelayCommand]
        private async Task DeleteAttendanceAsync(Attendance? attendance)
        {
            if (!CanManageAttendances || attendance == null)
            {
                return;
            }

            string studentName = attendance.StudentClass?.Student?.FullName ?? "Unknown";

            MessageResult result = _messageService.Show(
                $"តើអ្នកចង់លុបវត្តមានរបស់សិស្ស \"{studentName}\" ចុះថ្ងៃទី {attendance.AttendanceDate} ដែរឬទេ?",
                "លុបវត្តមាន",
                MessageButton.YesNo,
                MessageIcon.Question);

            if (result != MessageResult.Yes)
            {
                return;
            }

            IsLoading = true;

            try
            {
                ReturnResponse response = await _attendanceService.DeleteAsync(attendance);

                if (response.Status == Status.Success)
                {
                    _allAttendances.Remove(attendance);
                    ApplyFiltersAndPaging();
                    _messageService.Show("Attendance deleted successfully.", "Success", MessageButton.OK, MessageIcon.Success);
                }
                else
                {
                    _messageService.Show(response.Message ?? "Unable to delete the attendance.", "Error", MessageButton.OK, MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while deleting the attendance: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ResetFiltersAsync()
        {
            SearchText = string.Empty;
            ClassFilterId = null;
            StatusFilter = null;
            DateFromFilter = null;
            DateToFilter = null;
            CurrentPage = 1;
            ApplyFiltersAndPaging();
        }

        async partial void OnSearchTextChanged(string value)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            try
            {
                await Task.Delay(300, token);
                await RefreshOnFilterAsync();
            }
            catch (TaskCanceledException) { }
        }

        partial void OnClassFilterIdChanged(int? value)
        {
            CurrentPage = 1;
            ApplyFiltersAndPaging();
        }

        partial void OnStatusFilterChanged(AttendanceStatus? value)
        {
            CurrentPage = 1;
            ApplyFiltersAndPaging();
        }

        partial void OnDateFromFilterChanged(DateTime? value)
        {
            CurrentPage = 1;
            ApplyFiltersAndPaging();
        }

        partial void OnDateToFilterChanged(DateTime? value)
        {
            CurrentPage = 1;
            ApplyFiltersAndPaging();
        }

        partial void OnCurrentPageChanged(int oldValue, int newValue)
        {
            if (IsLoading)
            {
                return;
            }

            if (newValue < 1)
            {
                CurrentPage = 1;
                return;
            }

            if (newValue > MaxPage)
            {
                CurrentPage = MaxPage;
                return;
            }

            ApplyFiltersAndPaging();
        }

        private async Task LoadClassesAsync()
        {
            try
            {
                ReturnResponse<IEnumerable<Class>> response = await _classService.GetAllAsync(
                    filters: null,
                    page: 1,
                    pageSize: int.MaxValue,
                    orderBy: [new SortCriteria<Class>(c => c.Grade.KhmerName)],
                    includes: ["Grade"]);

                if (response.Status == Status.Success && response.Value != null)
                {
                    Classes.Clear();
                    foreach (Class cls in response.Value)
                    {
                        Classes.Add(cls);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading classes: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
        }

        private async Task LoadAttendancesAsync()
        {
            IsLoading = true;

            try
            {
                ReturnResponse<IEnumerable<Attendance>> response = await _attendanceService.GetAllAsync(
                    filters: null,
                    page: 1,
                    pageSize: int.MaxValue,
                    orderBy: [new SortCriteria<Attendance>("AttendanceDate", OrderDirection.Descending), new SortCriteria<Attendance>("ScanTime", OrderDirection.Descending)],
                    includes: ["StudentClass", "StudentClass.Student", "StudentClass.Student.Candidate", "StudentClass.Class", "MarkedByEmployee"]);

                if (response.Status != Status.Success || response.Value == null)
                {
                    _messageService.Show(response.Message ?? "Unable to load attendances.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                _allAttendances = new ObservableCollection<Attendance>(response.Value);
                CurrentPage = 1;
                ApplyFiltersAndPaging();
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading attendances: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFiltersAndPaging()
        {
            IEnumerable<Attendance> query = _allAttendances;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();
                query = query.Where(a =>
                    a.StudentClass?.Student?.FullName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                    a.StudentClass?.Student?.LatinFullName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (ClassFilterId.HasValue)
            {
                query = query.Where(a => a.StudentClass?.ClassId == ClassFilterId.Value);
            }

            if (StatusFilter.HasValue)
            {
                query = query.Where(a => a.Status == StatusFilter.Value);
            }

            if (DateFromFilter.HasValue)
            {
                DateOnly from = DateOnly.FromDateTime(DateFromFilter.Value);
                query = query.Where(a => a.AttendanceDate >= from);
            }

            if (DateToFilter.HasValue)
            {
                DateOnly to = DateOnly.FromDateTime(DateToFilter.Value);
                query = query.Where(a => a.AttendanceDate <= to);
            }

            List<Attendance> filteredAttendances = query
                .OrderByDescending(a => a.AttendanceDate)
                .ThenByDescending(a => a.ScanTime)
                .ToList();

            TotalCount = filteredAttendances.Count;
            MaxPage = Math.Max(1, (int)Math.Ceiling((double)TotalCount / DefaultPageSize));

            if (CurrentPage > MaxPage)
            {
                CurrentPage = MaxPage;
                return;
            }

            IEnumerable<Attendance> pageAttendances = filteredAttendances
                .Skip((CurrentPage - 1) * DefaultPageSize)
                .Take(DefaultPageSize);

            Attendances.Clear();
            foreach (Attendance attendance in pageAttendances)
            {
                Attendances.Add(attendance);
            }

            CurrentPageTotalCount = Attendances.Count;
            PageCount = $"ទំព័រ {CurrentPage} នៃ {MaxPage}";
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < MaxPage)
            {
                CurrentPage++;
            }
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        [RelayCommand]
        private async Task RefreshOnFilterAsync()
        {
            CurrentPage = 1;
            ApplyFiltersAndPaging();
        }
    }
}
