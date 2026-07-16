using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
        private CancellationTokenSource? _cts;
        private bool _isLoadingAttendances;

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
        private int _pageIndex = 1;

        [ObservableProperty]
        private string _pageCount = string.Empty;

        private (DateTime? Date, int? Id) _currentCursor = (null, null);
        private readonly Stack<(DateTime? Date, int? Id)> _previousCursors = new();

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
            ResetPagination();
            await LoadAttendancesAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            ResetPagination();
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
                    await LoadAttendancesAsync();
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
            ResetPagination();
            await LoadAttendancesAsync();
        }

        async partial void OnSearchTextChanged(string value)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            try
            {
                await Task.Delay(300, token);
                ResetPagination();
                _ = LoadAttendancesAsync();
            }
            catch (TaskCanceledException) { }
        }

        partial void OnClassFilterIdChanged(int? value)
        {
            ResetPagination();
            _ = LoadAttendancesAsync();
        }

        partial void OnStatusFilterChanged(AttendanceStatus? value)
        {
            ResetPagination();
            _ = LoadAttendancesAsync();
        }

        partial void OnDateFromFilterChanged(DateTime? value)
        {
            ResetPagination();
            _ = LoadAttendancesAsync();
        }

        partial void OnDateToFilterChanged(DateTime? value)
        {
            ResetPagination();
            _ = LoadAttendancesAsync();
        }

        private void ResetPagination()
        {
            _currentCursor = (null, null);
            _previousCursors.Clear();
            PageIndex = 1;
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
            if (_isLoadingAttendances)
            {
                return;
            }

            IsLoading = true;
            _isLoadingAttendances = true;

            try
            {
                List<FilterCondition<Attendance>> filters = BuildFilters();

                ReturnResponse<IEnumerable<Attendance>> response = await _attendanceService.GetAttendancesWithCursorAsync(
                    filters: filters,
                    lastDate: _currentCursor.Date,
                    lastId: _currentCursor.Id,
                    pageSize: DefaultPageSize,
                    includes: ["StudentClass", "StudentClass.Student", "StudentClass.Student.Candidate", "StudentClass.Class", "MarkedByEmployee"]);

                if (response.Status != Status.Success || response.Value == null)
                {
                    _messageService.Show(response.Message ?? "Unable to load attendances.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                Attendances.Clear();
                foreach (Attendance attendance in response.Value)
                {
                    Attendances.Add(attendance);
                }

                PageCount = $"ទំព័រ {PageIndex}";
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading attendances: {ex.Message}", "Error", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
                _isLoadingAttendances = false;
            }
        }

        private List<FilterCondition<Attendance>> BuildFilters()
        {
            var filters = new List<FilterCondition<Attendance>>();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filters.Add(new FilterCondition<Attendance>(
                    "StudentClass.Student.FullName",
                    FilterOperator.Contains,
                    SearchText.Trim()));
            }

            if (ClassFilterId.HasValue)
            {
                filters.Add(new FilterCondition<Attendance>(
                    "StudentClass.ClassId",
                    FilterOperator.Equals,
                    ClassFilterId.Value));
            }

            if (StatusFilter.HasValue)
            {
                filters.Add(new FilterCondition<Attendance>(
                    "Status",
                    FilterOperator.Equals,
                    StatusFilter.Value));
            }

            if (DateFromFilter.HasValue)
            {
                filters.Add(new FilterCondition<Attendance>(
                    "AttendanceDateTime",
                    FilterOperator.GreaterThanOrEqual,
                    DateFromFilter.Value));
            }

            if (DateToFilter.HasValue)
            {
                filters.Add(new FilterCondition<Attendance>(
                    "AttendanceDateTime",
                    FilterOperator.LessThanOrEqual,
                    DateToFilter.Value));
            }

            return filters;
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (Attendances.Count == DefaultPageSize)
            {
                var lastItem = Attendances.Last();
                _previousCursors.Push(_currentCursor);
                _currentCursor = (lastItem.AttendanceDateTime, lastItem.Id);
                PageIndex++;
                await LoadAttendancesAsync();
            }
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (_previousCursors.Count > 0)
            {
                _currentCursor = _previousCursors.Pop();
                PageIndex--;
                await LoadAttendancesAsync();
            }
        }
    }
}
