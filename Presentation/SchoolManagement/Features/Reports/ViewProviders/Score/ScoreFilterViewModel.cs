using System.Collections.Specialized;
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
        private readonly IMessageService _messageService;
        private readonly IDispatcherService _dispatcherService;

        public ObservableCollection<SelectableClass> SelectableClasses { get; } = [];

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
            IAuthorizationService authorizationService,
            IMessageService messageService,
            IDispatcherService dispatcherService)
        {
            _classService = classService;
            _classSubjectService = classSubjectService;
            _examService = examService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _dispatcherService = dispatcherService;

            SelectableClasses.CollectionChanged += OnSelectableClassesChanged;
        }

        public override ScoreReportFilter GetFilterData()
        {
            var classIds = SelectableClasses
                .Where(sc => sc.IsSelected)
                .Select(sc => sc.Class.Id)
                .ToList();

            return new ScoreReportFilter
            {
                ClassIds = classIds,
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

            IEnumerable<Class> loadedClasses;

            if (user.IsValidRole(RoleType.Admin))
            {
                loadedClasses = (await _classService.GetAllAsync()).Value ?? [];
            }
            else if (user.IsValidRole(RoleType.HeadTeacher))
            {
                loadedClasses = (await _classService.GetAllAsync(
                    filters: [new(c => c.Generation.DepartmentId, FilterOperator.Equals, user.Employee?.DepartmentId)],
                    includes: ["Generation"])).Value ?? [];
            }
            else if (user.IsValidRole(RoleType.Teacher))
            {
                loadedClasses = (await _classService.GetAllAsync(
                    filters: [new(c => c.TeacherId, FilterOperator.Equals, user.EmployeeId)],
                    includes: ["Employee"])).Value ?? [];
            }
            else
            {
                loadedClasses = [];
            }

            SelectableClasses.CollectionChanged -= OnSelectableClassesChanged;
            SelectableClasses.Clear();
            SelectableClasses.CollectionChanged += OnSelectableClassesChanged;
            foreach (var cls in loadedClasses)
                SelectableClasses.Add(new SelectableClass(cls));
        }

        private bool _isResetting;
        private int _loadSubjectsCounter;

        private void OnSelectableClassesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SelectableClass item in e.NewItems)
                    item.PropertyChanged += (_, _) => {
                        if (_isResetting) return;
                        _ = LoadSubjectsAsync();
                        OnFilterChanged();
                    };
            }
        }

        partial void OnSearchKeywordChanged(string value)
        {
            ScheduleDebouncedFilter();
        }

        partial void OnSelectedSubjectChanged(ClassSubject? value)
        {
            if (_isResetting) return;
            OnFilterChanged();
        }

        private async Task LoadSubjectsAsync()
        {
            int currentCount = System.Threading.Interlocked.Increment(ref _loadSubjectsCounter);
            await Task.Delay(100);
            if (currentCount != _loadSubjectsCounter) return;

            var classIds = SelectableClasses
                .Where(sc => sc.IsSelected)
                .Select(sc => sc.Class.Id)
                .ToList();

            if (classIds.Count > 0)
            {
                var response = await _classSubjectService.GetAllAsync(
                    filters: [new FilterCondition<ClassSubject>(cs => cs.ClassId, FilterOperator.In, classIds.Cast<object>())],
                    includes: ["Subject"]);

                if (response.Status != Status.Success)
                {
                    _dispatcherService.Invoke(() =>
                    {
                        _messageService.Show("មានកំហុសបច្ចេកទេសអំឡុងពេលដែលកំពុងទាញយកទិន្នន័យមុខវិជ្ជា");
                    });
                }

                var distinctSubjects = response.Value?
                    .GroupBy(cs => cs.SubjectId)
                    .Select(g => g.First())
                    .ToList() ?? [];

                Subjects = distinctSubjects;
            }
            else
            {
                Subjects = [];
            }
        }

        public override void ResetFilterData()
        {
            _isResetting = true;
            try
            {
                foreach (var sc in SelectableClasses)
                    sc.IsSelected = false;
                SelectedSubject = null;
                SelectedExam = null;
                SearchKeyword = string.Empty;
            }
            finally
            {
                _isResetting = false;
                _ = LoadSubjectsAsync();
                OnFilterChanged();
            }
        }
    }

    public partial class SelectableClass : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        public Class Class { get; }

        public SelectableClass(Class cls, bool isSelected = false)
        {
            Class = cls;
            _isSelected = isSelected;
        }
    }
}
