using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Exams.Models;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.Score
{
    public partial class ScoreFilterViewModel : ReportFilterViewModelBase<ScoreReportFilter>
    {
        private readonly IClassService _classService;
        private readonly IClassSubjectService _classSubjectService;
        private readonly IExamService _examService;
        private readonly IAuthorizationService _authorizationService;

        [ObservableProperty]
        private IEnumerable<Class> _classes = [];

        [ObservableProperty]
        private Class? _selectedClass;

        [ObservableProperty]
        private IEnumerable<ClassSubject> _subjects = [];

        [ObservableProperty]
        private ClassSubject? _selectedSubject;

        [ObservableProperty]
        private IEnumerable<Exam> _exams = [];

        [ObservableProperty]
        private Exam? _selectedExam;

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        public override string ReportTypeKey => "score-report";

        public ScoreFilterViewModel(
            IClassService classService,
            IClassSubjectService classSubjectService,
            IExamService examService,
            IAuthorizationService authorizationService)
        {
            _classService = classService;
            _classSubjectService = classSubjectService;
            _examService = examService;
            _authorizationService = authorizationService;
        }

        public override ScoreReportFilter GetFilterData()
        {
            return new ScoreReportFilter
            {
                ClassId = SelectedClass?.Id,
                SubjectId = SelectedSubject?.SubjectId,
                ExamId = SelectedExam?.Id,
                SearchKeyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
            };
        }

        public override async Task LoadAsync()
        {
            User? user = _authorizationService.CurrentUser;
            if (user == null) return;

            Exams = (await _examService.GetAllAsync()).Value ?? [];

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

        partial void OnSelectedClassChanged(Class? value)
        {
            _ = LoadSubjectsAsync(value?.Id);      
        }

        partial void OnSelectedSubjectChanged(ClassSubject? value)
        {  
        
        }

        partial void OnSelectedExamChanged(Exam? value)
        {      
        
        }

        partial void OnSearchKeywordChanged(string value)
        {
            ScheduleDebouncedFilter();
        }

        private async Task LoadSubjectsAsync(int? classId)
        {
            if (classId.HasValue)
            {
                var response = await _classSubjectService.GetAllAsync(
                    filters: [new FilterCondition<ClassSubject>(cs => cs.ClassId, FilterOperator.Equals, classId.Value)],
                    includes: ["Subject"]);
                Subjects = response.Value ?? [];
            }
            else
            {
                Subjects = [];
            }
        }

        public override void ResetFilterData()
        {
            SelectedClass = null;
            SelectedSubject = null;
            SelectedExam = null;
            SearchKeyword = string.Empty;
        }
    }
}
