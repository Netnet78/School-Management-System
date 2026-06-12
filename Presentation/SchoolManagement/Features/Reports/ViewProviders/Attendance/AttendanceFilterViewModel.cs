using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.Attendance
{
    public partial class AttendanceFilterViewModel : ObservableObject, IReportFilterViewModel, IAsyncLoadable
    {
        private readonly IClassService _classService;
        private readonly IAuthorizationService _authorizationService;
        private Timer? _searchDebounceTimer;

        public ReportTag ReportTypeKey => ReportTag.AttendanceReport;

        public event Action? FilterChanged;

        [ObservableProperty]
        private IEnumerable<Class> _classes = [];

        [ObservableProperty]
        private Class? _selectedClass;

        [ObservableProperty]
        private DateTime _dateFrom = new(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [ObservableProperty]
        private DateTime _dateTo = DateTime.UtcNow;

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        public AttendanceFilterViewModel(IClassService classService, IAuthorizationService authorizationService)
        {
            _classService = classService;
            _authorizationService = authorizationService;
        }

        public object GetFilterData()
        {
            return new AttendanceReportFilter
            {
                ClassId = SelectedClass?.Id,
                DateFrom = DateFrom,
                DateTo = DateTo,
                SearchKeyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
            };
        }

        public async Task LoadAsync()
        {
            User? user = _authorizationService.CurrentUser;
            if (user == null) return;

            if (user.IsValidRole(RoleType.Admin))
            {
                Classes = (await _classService.GetAllAsync()).Value ?? [];
            }
            else if (user.IsValidRole(RoleType.HeadTeacher))
            {
                Classes = (await _classService.GetAllAsync(
                    filters: [new(c => c.Generation.DepartmentId, FilterOperator.Equals, user.Employee?.DepartmentId)],
                    includes: ["Generation"])).Value ?? [];
            }
            else if (user.IsValidRole(RoleType.Teacher))
            {
                Classes = (await _classService.GetAllAsync(
                    filters: [new(c => c.TeacherId, FilterOperator.Equals, user.EmployeeId)],
                    includes: ["Employee"])).Value ?? [];
            }
        }

        partial void OnSelectedClassChanged(Class? value) => FilterChanged?.Invoke();
        partial void OnDateFromChanged(DateTime value) => FilterChanged?.Invoke();
        partial void OnDateToChanged(DateTime value) => FilterChanged?.Invoke();

        partial void OnSearchKeywordChanged(string value)
        {
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(
                async (_) => { FilterChanged?.Invoke(); },
                null,
                400,
                Timeout.Infinite);
        }

        public void ResetFilterData()
        {
            SelectedClass = null;
            DateFrom = new(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTo = DateTime.UtcNow;
            SearchKeyword = string.Empty;
        }
    }
}
