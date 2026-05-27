using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Features.Exams.Models;
using SchoolManagement.Infrastructure.Features.Classes.Contracts;
using SchoolManagement.Infrastructure.Features.Exams.Contracts;
using SchoolManagement.Infrastructure.Features.Shared.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.Score
{
    public partial class ScoreFilterViewModel : ObservableObject, IReportFilterViewModel, IAsyncLoadable
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassSubjectRepository _classSubjectRepository;
        private readonly IExamRepository _examRepository;

        public string ReportTypeKey => "score-report";

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

        public ScoreFilterViewModel(
            IClassRepository classRepository,
            IClassSubjectRepository classSubjectRepository,
            IExamRepository examRepository)
        {
            _classRepository = classRepository;
            _classSubjectRepository = classSubjectRepository;
            _examRepository = examRepository;
        }

        public object GetFilterData()
        {
            return new ScoreReportFilter
            {
                ClassId = SelectedClass?.Id,
                SubjectId = SelectedSubject?.SubjectId,
                ExamId = SelectedExam?.Id,
            };
        }

        public async Task LoadAsync()
        {
            Classes = (await _classRepository.GetAllAsync()).ToList();
            Exams = (await _examRepository.GetAllAsync()).ToList();
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

        private async Task LoadSubjectsAsync(int? classId)
        {
            if (classId.HasValue)
            {
                var allSubjects = await _classSubjectRepository.FindAsync(
                    new[] { new FilterCondition<ClassSubject>(cs => cs.ClassId, FilterOperator.Equals, classId.Value) },
                    includes: "Subject");
                Subjects = allSubjects.ToList();
            }
            else
            {
                Subjects = [];
            }
        }
    }
}
