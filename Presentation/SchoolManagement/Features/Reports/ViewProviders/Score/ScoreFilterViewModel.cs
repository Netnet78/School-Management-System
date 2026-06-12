using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Core.Features.Exams.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.Score
{
    public partial class ScoreFilterViewModel : ObservableObject, IReportFilterViewModel, IAsyncLoadable
    {
        private readonly IClassService _classService;
        private readonly IClassSubjectService _classSubjectService;
        private readonly IExamService _examService;
        private readonly IAuthorizationService _authorizationService;
        private Timer? _searchDebounceTimer;

        public ReportTag ReportTypeKey => ReportTag.ScoreReport;

        public event Action? FilterChanged;

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

        public object GetFilterData()
        {
            return new ScoreReportFilter
            {
                ClassId = SelectedClass?.Id,
                SubjectId = SelectedSubject?.SubjectId,
                ExamId = SelectedExam?.Id,
                SearchKeyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
            };
        }

        public async Task LoadAsync()
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
            FilterChanged?.Invoke();
        }

        partial void OnSelectedSubjectChanged(ClassSubject? value)
        {
            FilterChanged?.Invoke();
        }

        partial void OnSelectedExamChanged(Exam? value)
        {
            FilterChanged?.Invoke();
        }

        partial void OnSearchKeywordChanged(string value)
        {
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(
                async (_) => { FilterChanged?.Invoke(); },
                null,
                400,
                Timeout.Infinite);
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

        public void ResetFilterData()
        {
            SelectedClass = null;
            SelectedSubject = null;
            SelectedExam = null;
            SearchKeyword = string.Empty;
        }
    }
}
