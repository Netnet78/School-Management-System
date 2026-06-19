using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Application.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.Attendance
{
    public partial class AttendanceFilterViewModel : ReportFilterViewModelBase<AttendanceReportFilter>
    {
        private readonly IClassService _classService;
        private readonly IAuthorizationService _authorizationService;

        public override string ReportTypeKey => "attendance-report";

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

        public override AttendanceReportFilter GetFilterData()
        {
            return new()
            {
                ClassId = SelectedClass?.Id,
                DateFrom = DateFrom,
                DateTo = DateTo,
                SearchKeyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
            };
        }

        public override async Task LoadAsync()
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

        partial void OnSelectedClassChanged(Class? value) => OnFilterChanged();
        partial void OnDateFromChanged(DateTime value) => OnFilterChanged();
        partial void OnDateToChanged(DateTime value) => OnFilterChanged();
        partial void OnSearchKeywordChanged(string value) => ScheduleDebouncedFilter();

        public override void ResetFilterData()
        {
            SelectedClass = null;
            DateFrom = new(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTo = DateTime.UtcNow;
            SearchKeyword = string.Empty;
        }
    }
}
