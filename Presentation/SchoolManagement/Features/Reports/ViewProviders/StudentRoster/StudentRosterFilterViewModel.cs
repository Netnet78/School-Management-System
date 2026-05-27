using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Features.Grades.Models;
using SchoolManagement.Core.Features.Skills.Models;
using SchoolManagement.Infrastructure.Features.Grades.Contracts;
using SchoolManagement.Infrastructure.Features.Classes.Contracts;
using SchoolManagement.Infrastructure.Features.Skills.Contracts;
using SchoolManagement.Infrastructure.Features.Shared.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.StudentRoster
{
    public partial class StudentRosterFilterViewModel : ObservableObject, IReportFilterViewModel, IAsyncLoadable
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IClassRepository _classRepository;
        private readonly ISkillRepository _skillRepository;

        public string ReportTypeKey => "student-roster";

        public event Action? FilterChanged;

        [ObservableProperty]
        private IEnumerable<Grade> _grades = [];

        [ObservableProperty]
        private Grade? _selectedGrade;

        [ObservableProperty]
        private IEnumerable<Class> _classes = [];

        [ObservableProperty]
        private Class? _selectedClass;

        [ObservableProperty]
        private IEnumerable<Skill> _skills = [];

        [ObservableProperty]
        private Skill? _selectedSkill;

        [ObservableProperty]
        private bool _includeInactive;

        public StudentRosterFilterViewModel(
            IGradeRepository gradeRepository,
            IClassRepository classRepository,
            ISkillRepository skillRepository)
        {
            _gradeRepository = gradeRepository;
            _classRepository = classRepository;
            _skillRepository = skillRepository;
        }

        public object GetFilterData()
        {
            return new StudentRosterFilter
            {
                GradeId = SelectedGrade?.Id,
                ClassId = SelectedClass?.Id,
                SkillId = SelectedSkill?.Id,
                IsActive = !IncludeInactive,
            };
        }

        public async Task LoadAsync()
        {
            Grades = (await _gradeRepository.GetAllAsync()).ToList();
            Skills = (await _skillRepository.GetAllAsync()).ToList();
            Classes = (await _classRepository.GetAllAsync()).ToList();
        }

        partial void OnSelectedGradeChanged(Grade? value)
        {
            FilterChanged?.Invoke();
        }

        partial void OnSelectedClassChanged(Class? value)
        {
            FilterChanged?.Invoke();
        }

        partial void OnSelectedSkillChanged(Skill? value)
        {
            FilterChanged?.Invoke();
        }

        partial void OnIncludeInactiveChanged(bool value)
        {
            FilterChanged?.Invoke();
        }
    }
}
