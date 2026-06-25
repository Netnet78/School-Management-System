using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Core.Features.Exams.Models;
using SchoolManagement.Presentation.Features.Classes.ViewModels;
using SchoolManagement.Presentation.Features.Dashboard.ViewModels;
using SchoolManagement.Presentation.Shared.Features.Scores.Observables;
using SchoolManagement.Presentation.Shared.Features.Scores.Params;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Scores.ViewModels
{
    public partial class ScoreViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly IStudentClassService _studentClassService;
        private readonly IExamService _examService;
        private readonly IAssessmentService _scoreService;
        private readonly IClassSubjectService _classSubjectService;
        private readonly INavigationService _navigationService;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        private Class? _preSelectedClass;
        private List<int> _studentClassIds = [];
        private Dictionary<int, int> _studentIdToStudentClassId = [];

        // ====== Mode & UI state ======
        [ObservableProperty]
        private bool _isClassSelectionMode;

        [ObservableProperty]
        private bool _isScoreEntryMode;

        [ObservableProperty]
        private bool _isLoadingClasses;

        [ObservableProperty]
        private bool _isLoadingScores;

        [ObservableProperty]
        private bool _isSavingScores;

        [ObservableProperty]
        private bool _hasScores;

        // ====== Class selection ======
        [ObservableProperty]
        private ObservableCollection<Class> _classes = [];

        [ObservableProperty]
        private Class? _selectedClass;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _classInfo = string.Empty;

        // ====== Subject selection ======
        [ObservableProperty]
        private ObservableCollection<ClassSubject> _scoreSubjects = [];

        [ObservableProperty]
        private ClassSubject? _selectedScoreSubject;

        // ====== Exam selection ======
        [ObservableProperty]
        private ObservableCollection<Exam> _exams = [];

        [ObservableProperty]
        private Exam? _selectedExam;

        // ====== Bulk scores ======
        [ObservableProperty]
        private ObservableCollection<StudentScoreEntry> _scoreEntries = [];

        // ====== Permissions ======
        [ObservableProperty]
        private bool _canEditScores;

        public ScoreViewModel(
            IStudentClassService studentClassService,
            IExamService examService,
            IAssessmentService scoreService,
            IClassSubjectService classSubjectService,
            INavigationService navigationService,
            IMessageService messageService,
            IAuthorizationService authorizationService)
        {
            _studentClassService = studentClassService;
            _examService = examService;
            _scoreService = scoreService;
            _classSubjectService = classSubjectService;
            _navigationService = navigationService;
            _messageService = messageService;
            _authorizationService = authorizationService;
        }

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is ScoreNavigationParams p && p.Class != null)
            {
                _preSelectedClass = p.Class;
                ClassName = _preSelectedClass.GetKhmerName();
                ClassInfo = $"ថ្នាក់: {_preSelectedClass.GetKhmerName()} | កម្រិត: {_preSelectedClass.Grade?.KhmerName} | ជំនាន់: {_preSelectedClass.Generation?.CohortNumber} | ផ្នែក: {_preSelectedClass.Generation?.Department?.KhmerName}";
            }
        }

        public async Task LoadAsync()
        {
            User? user = _authorizationService.CurrentUser;
            if (user == null)
            {
                _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                return;
            }

            CanEditScores = (await _authorizationService.AuthorizeAsync(null, PermissionType.ManageAssessments)).Status == Status.Success;

            await LoadExamsAsync();

            if (_preSelectedClass != null)
            {
                IsClassSelectionMode = false;
                IsScoreEntryMode = true;
                SelectedClass = _preSelectedClass;
                await LoadSubjectsForClassAsync(_preSelectedClass.Id);
            }
            else
            {
                IsClassSelectionMode = true;
                IsScoreEntryMode = false;
                await LoadTeacherClassesAsync();
            }
        }

        // ====== Load teacher's classes ======

        private async Task LoadTeacherClassesAsync()
        {
            IsLoadingClasses = true;

            try
            {
                User? user = _authorizationService.CurrentUser;
                if (user?.EmployeeId == null)
                {
                    // Admin/HeadTeacher — load all classes
                    var allClassesResponse = await _classSubjectService.GetAllAsync(includes: ["Class.Grade", "Class.Generation.Department"]);
                    if (allClassesResponse.Status == Status.Success && allClassesResponse.Value != null)
                    {
                        var classIds = new HashSet<int>();
                        Classes.Clear();
                        foreach (var cs in allClassesResponse.Value)
                        {
                            if (cs.Class != null && classIds.Add(cs.Class.Id))
                            {
                                Classes.Add(cs.Class);
                            }
                        }
                    }
                    return;
                }

                // Teacher — load only classes they teach
                var response = await _classSubjectService.GetAllAsync(
                    filters: [new FilterCondition<ClassSubject>(cs => cs.TeacherId, FilterOperator.Equals, user.EmployeeId)],
                    includes: ["Class.Grade", "Class.Generation.Department"]);

                if (response.Status == Status.Success && response.Value != null)
                {
                    var classIds = new HashSet<int>();
                    Classes.Clear();
                    foreach (var cs in response.Value)
                    {
                        if (cs.Class != null && classIds.Add(cs.Class.Id))
                        {
                            Classes.Add(cs.Class);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យថ្នាក់: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoadingClasses = false;
            }
        }

        async partial void OnSelectedClassChanged(Class? value)
        {
            if (value == null) return;

            ClassName = value.GetKhmerName();
            ClassInfo = $"ថ្នាក់: {value.GetKhmerName()} | កម្រិត: {value.Grade?.KhmerName} | ជំនាន់: {value.Generation?.CohortNumber} | ផ្នែក: {value.Generation?.Department?.KhmerName}";

            IsClassSelectionMode = false;
            IsScoreEntryMode = true;

            await LoadSubjectsForClassAsync(value.Id);
            ClearScoreEntries();
        }

        private async Task LoadSubjectsForClassAsync(int classId)
        {
            User? user = _authorizationService.CurrentUser;
            List<FilterCondition<ClassSubject>> filters = [new FilterCondition<ClassSubject>(cs => cs.ClassId, FilterOperator.Equals, classId)];

            // If teacher, filter by their subjects
            if (user?.EmployeeId != null && !user.IsAdmin() && !user.IsHeadTeacher())
            {
                filters.Add(new FilterCondition<ClassSubject>(cs => cs.TeacherId, FilterOperator.Equals, user.EmployeeId));
            }

            try
            {
                var response = await _classSubjectService.GetAllAsync(
                    filters: filters,
                    includes: ["Subject"]);

                if (response.Status == Status.Success && response.Value != null)
                {
                    ScoreSubjects = new ObservableCollection<ClassSubject>(response.Value);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យមុខវិជ្ជា: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
        }

        // ====== Load exams ======

        private async Task LoadExamsAsync()
        {
            try
            {
                var response = await _examService.GetAllAsync(1);
                if (response.Status == Status.Success && response.Value != null)
                {
                    Exams = new ObservableCollection<Exam>(response.Value);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យប្រឡង: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
        }

        // ====== Load bulk score entries ======

        async partial void OnSelectedExamChanged(Exam? value)
        {
            await LoadScoreEntriesAsync();
        }

        async partial void OnSelectedScoreSubjectChanged(ClassSubject? value)
        {
            await LoadScoreEntriesAsync();
        }

        private async Task LoadScoreEntriesAsync()
        {
            if (SelectedExam == null || SelectedScoreSubject == null || SelectedClass == null)
            {
                ScoreEntries.Clear();
                HasScores = false;
                return;
            }

            IsLoadingScores = true;

            try
            {
                // 1. Load students in the class
                var scResponse = await _studentClassService.GetAllAsync(
                    filters: [new FilterCondition<StudentClass>(sc => sc.ClassId, FilterOperator.Equals, SelectedClass.Id)],
                    page: 1,
                    includes: ["Student.Candidate", "Student.Candidate.Skill"]);

                if (scResponse.Status != Status.Success || scResponse.Value == null)
                {
                    ScoreEntries.Clear();
                    HasScores = false;
                    return;
                }

                List<StudentClass> studentClasses = scResponse.Value.ToList();
                _studentClassIds = studentClasses.Select(sc => sc.Id).ToList();
                _studentIdToStudentClassId = studentClasses
                    .Where(sc => sc.Student != null)
                    .ToDictionary(sc => sc.Student!.Id, sc => sc.Id);

                // 2. Load existing assessments for this exam + subject + class
                var existingResponse = await _scoreService.GetAllAsync(
                    filters: [
                        new FilterCondition<Assessment>(s => s.ExamId, FilterOperator.Equals, SelectedExam.Id),
                        new FilterCondition<Assessment>(s => s.ClassSubjectId, FilterOperator.Equals, SelectedScoreSubject.Id),
                        new FilterCondition<Assessment>(s => s.StudentClassId, FilterOperator.In, _studentClassIds.Cast<object>())
                    ],
                    includes: ["StudentClass.Student.Candidate"]);

                var existingByScId = new Dictionary<int, Assessment>();
                if (existingResponse.Status == Status.Success && existingResponse.Value != null)
                {
                    foreach (var a in existingResponse.Value)
                    {
                        existingByScId[a.StudentClassId] = a;
                    }
                }

                // 3. Build score entries
                ScoreEntries.Clear();
                foreach (var sc in studentClasses)
                {
                    if (sc.Student == null) continue;

                    existingByScId.TryGetValue(sc.Id, out Assessment? existing);
                    ScoreEntries.Add(new StudentScoreEntry(sc.Student, sc.Id, existing));
                }

                HasScores = ScoreEntries.Count > 0;
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យពិន្ទុ: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoadingScores = false;
            }
        }

        // ====== Save All ======

        [RelayCommand]
        private async Task SaveAllAsync()
        {
            if (SelectedExam == null || SelectedScoreSubject == null)
            {
                _messageService.Show("សូមជ្រើសរើសប្រឡង និងមុខវិជ្ជា!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            // Validate against MaxScore
            decimal maxScore = SelectedScoreSubject.Subject?.MaxScore ?? 0;
            foreach (var entry in ScoreEntries)
            {
                if (entry.ScoreAmount < 0)
                {
                    _messageService.Show($"ពិន្ទុរបស់ {entry.Student.FullName} មិនត្រឹមត្រូវទេ!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                    return;
                }

                if (maxScore > 0 && entry.ScoreAmount > maxScore)
                {
                    _messageService.Show($"ពិន្ទុរបស់ {entry.Student.FullName} លើសពី {maxScore}!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                    return;
                }
            }

            IsSavingScores = true;

            try
            {
                var entries = ScoreEntries
                    .Where(e => e.ScoreAmount >= 0)
                    .Select(e => (e.StudentClassId, e.ScoreAmount));

                var response = await _scoreService.UpsertRangeAsync(SelectedExam.Id, SelectedScoreSubject.Id, entries);

                if (response.Status == Status.Success)
                {
                    _messageService.Show("បានរក្សាទុកពិន្ទុទាំងអស់ដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);
                    await LoadScoreEntriesAsync();
                }
                else
                {
                    _messageService.Show(response.Message ?? "មានកំហុសក្នុងការរក្សាទុក!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសបច្ចេកទេស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsSavingScores = false;
            }
        }

        // ====== Change class (from score entry mode) ======

        [RelayCommand]
        private void ChangeClass()
        {
            ClearScoreEntries();
            SelectedClass = null;
            SelectedExam = null;
            SelectedScoreSubject = null;
            IsScoreEntryMode = false;
            IsClassSelectionMode = true;
        }

        private void ClearScoreEntries()
        {
            ScoreEntries.Clear();
            HasScores = false;
            _studentClassIds.Clear();
            _studentIdToStudentClassId.Clear();
        }

        // ====== Report ======

        [RelayCommand]
        private async Task GenerateReportAsync()
        {
            if (ScoreEntries.Count == 0)
            {
                _messageService.Show("មិនមានទិន្នន័យដើម្បីធ្វើរបាយការណ៍ទេ!", "ព័ត៌មាន", MessageButton.OK, MessageIcon.Information);
                return;
            }

            string report = $"=== របាយការណ៍ថ្នាក់: {ClassName} ===\n\n";
            report += $"ចំនួនសិស្សសរុប: {ScoreEntries.Count} នាក់\n";

            if (SelectedExam != null && SelectedScoreSubject != null)
            {
                report += $"ប្រឡង: {SelectedExam.KhmerName}\n";
                report += $"មុខវិជ្ជា: {SelectedScoreSubject.Subject?.KhmerName}\n";

                var scoredEntries = ScoreEntries.Where(e => e.ScoreAmount >= 0).ToList();
                report += $"ចំនួនពិន្ទុ: {scoredEntries.Count}\n";
                if (scoredEntries.Count > 0)
                {
                    report += $"ពិន្ទុមធ្យម: {scoredEntries.Average(e => (double)e.ScoreAmount):F2}\n";
                    report += $"ពិន្ទុខ្ពស់បំផុត: {scoredEntries.Max(e => e.ScoreAmount)}\n";
                    report += $"ពិន្ទុទាបតិចបំផុត: {scoredEntries.Min(e => e.ScoreAmount)}\n\n";

                    report += "បញ្ជីពិន្ទុ:\n";
                    foreach (var entry in scoredEntries.OrderBy(e => e.Student.FullName))
                    {
                        report += $"- {entry.Student.FullName}: {entry.ScoreAmount}\n";
                    }
                }
            }

            _messageService.Show(report, $"របាយការណ៍: {ClassName}", MessageButton.OK, MessageIcon.Information);
        }

        // ====== Navigation ======

        [RelayCommand]
        private async Task GoBackAsync()
        {
            IViewModel? previous = _navigationService.PreviousViewModel;
            if (previous != null)
            {
                await _navigationService.NavigateAsync(previous.GetType());
            }
            else
            {
                await _navigationService.NavigateAsync<DashboardViewModel>();
            }
        }
    }
}
