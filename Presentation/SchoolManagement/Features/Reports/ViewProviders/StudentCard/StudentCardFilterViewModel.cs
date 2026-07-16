using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Application.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.StudentCard
{
    public partial class StudentCardFilterViewModel : ReportFilterViewModelBase<StudentCardFilter>
    {
        private readonly IClassService _classService;
        private readonly IAuthorizationService _authorizationService;

        public override string ReportTypeKey => "student-card";

        [ObservableProperty]
        private IEnumerable<Class> _classes = [];

        [ObservableProperty]
        private Class? _selectedClass;

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        partial void OnSelectedClassChanged(Class? value) => ScheduleDebouncedFilter();
        partial void OnSearchKeywordChanged(string value) => ScheduleDebouncedFilter();

        public StudentCardFilterViewModel(
            IClassService classService,
            IAuthorizationService authorizationService)
        {
            _classService = classService;
            _authorizationService = authorizationService;
        }

        public override StudentCardFilter GetFilterData()
        {
            return new()
            {
                ClassId = SelectedClass?.Id,
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
                Classes = (await _classService.GetAllAsync(1, null,
                [new(c => c.Generation.DepartmentId, FilterOperator.Equals, user.Employee?.DepartmentId)],
                includes: ["Generation"])).Value ?? [];
            }
            else if (user.IsValidRole(RoleType.Teacher))
            {
                Classes = (await _classService.GetAllAsync(1, null,
                [new(c => c.TeacherId, FilterOperator.Equals, user.EmployeeId)],
                includes: ["Employee"])).Value ?? [];
            }
        }

        public override void ResetFilterData()
        {
            SelectedClass = null;
            SearchKeyword = string.Empty;
        }
    }
}
