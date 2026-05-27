using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Features.Accessments.Models;
using SchoolManagement.Core.Features.Exams.Models;
using SchoolManagement.Presentation.Features.Classes.ViewModels;
using SchoolManagement.Presentation.Shared.Features.Scores.Params;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Features.Scores.ViewModels
{
    public partial class ScoreViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly IStudentClassService _studentClassService;
        private readonly IExamService _examService;
        private readonly IAccessmentService _scoreService;
        private readonly IClassSubjectService _classSubjectService;
        private readonly INavigationService _navigationService;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        private Class _class = null!;
        private List<int> _studentClassIds = [];
        private Dictionary<int, int> _studentIdToStudentClassId = [];

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _classInfo = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Student> _students = [];

        [ObservableProperty]
        private bool _isLoadingStudents;

        [ObservableProperty]
        private ObservableCollection<Exam> _exams = [];

        [ObservableProperty]
        private Exam? _selectedExam;

        [ObservableProperty]
        private ObservableCollection<Assessment> _scores = [];

        [ObservableProperty]
        private bool _isLoadingScores;

        [ObservableProperty]
        private bool _hasScores;

        [ObservableProperty]
        private bool _canEditStudents;

        [ObservableProperty]
        private bool _showScoreForm;

        [ObservableProperty]
        private string _scoreStudentName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ClassSubject> _scoreSubjects = [];

        [ObservableProperty]
        private ClassSubject? _selectedScoreSubject;

        [ObservableProperty]
        private Exam? _selectedScoreExam;

        [ObservableProperty]
        private decimal _scoreAmount;

        [ObservableProperty]
        private bool _isSavingScore;

        private Student? _selectedStudentForScore;

        public ScoreViewModel(
            IStudentClassService studentClassService,
            IExamService examService,
            IAccessmentService scoreService,
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
            if (@params is not ScoreNavigationParams p || p.Class == null)
            {
                _messageService.Show("មិនអាចរកឃើញទិន្នន័យថ្នាក់!", "មានកំហុស!", MessageButton.OK, MessageIcon.Error);
                return;
            }

            _class = p.Class;
            ClassName = _class.GetKhmerName();
            ClassInfo = $"ថ្នាក់: {_class.GetKhmerName()} | កម្រិត: {_class.Grade?.KhmerName} | ជំនាន់: {_class.Generation?.CohortNumber} | ផ្នែក: {_class.Generation?.Department?.KhmerName}";
        }

        public async Task LoadAsync()
        {
            if (_class == null) return;

            User? user = _authorizationService.CurrentUser;
            if (user == null)
            {
                _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                return;
            }
            CanEditStudents = (await _authorizationService.AuthorizeAsync(null, PermissionType.EditStudents)).Status == Status.Success;

            await LoadStudentsAsync();
            await LoadExamsAsync();
        }

        private async Task LoadStudentsAsync()
        {
            IsLoadingStudents = true;

            try
            {
                var scResponse = await _studentClassService.GetAllAsync(
                    filters: [new FilterCondition<StudentClass>(sc => sc.ClassId, FilterOperator.Equals, _class.Id)],
                    page: 1,
                    includes: ["Student.Candidate", "Student.Candidate.Skill"]);

                Students.Clear();
                _studentClassIds.Clear();
                _studentIdToStudentClassId.Clear();

                if (scResponse.Status == Status.Success && scResponse.Value != null)
                {
                    foreach (StudentClass sc in scResponse.Value)
                    {
                        if (sc.Student != null)
                        {
                            Students.Add(sc.Student);
                            _studentClassIds.Add(sc.Id);
                            _studentIdToStudentClassId[sc.Student.Id] = sc.Id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យសិស្ស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoadingStudents = false;
            }
        }

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

        async partial void OnSelectedExamChanged(Exam? value)
        {
            if (value == null || _studentClassIds.Count == 0)
            {
                Scores.Clear();
                HasScores = false;
                return;
            }

            await LoadScoresAsync(value.Id);
        }

        private async Task LoadScoresAsync(int examId)
        {
            IsLoadingScores = true;

            try
            {
                var response = await _scoreService.GetAllAsync(
                    filters: [
                        new FilterCondition<Assessment>(s => s.ExamId, FilterOperator.Equals, examId),
                        new FilterCondition<Assessment>(s => s.StudentClassId, FilterOperator.In, _studentClassIds.Cast<object>())
                    ],
                    includes: ["StudentClass.Student.Candidate", "ClassSubject.Subject"]);

                if (response.Status == Status.Success && response.Value != null)
                {
                    Scores = new ObservableCollection<Assessment>(response.Value);
                    HasScores = Scores.Count > 0;
                }
                else
                {
                    Scores.Clear();
                    HasScores = false;
                }
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

        [RelayCommand]
        private async Task AddScoreAsync(Student? student)
        {
            if (student == null) return;

            if (!_studentIdToStudentClassId.ContainsKey(student.Id))
            {
                _messageService.Show("សិស្សនេះមិនមានក្នុងថ្នាក់ទេ!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                return;
            }

            _selectedStudentForScore = student;
            ScoreStudentName = student.FullName;
            SelectedScoreExam = null;
            SelectedScoreSubject = null;
            ScoreAmount = 0;

            try
            {
                var csResponse = await _classSubjectService.GetAllAsync(
                    filters: [new FilterCondition<ClassSubject>(cs => cs.ClassId, FilterOperator.Equals, _class.Id)],
                    includes: ["Subject"]);

                if (csResponse.Status == Status.Success && csResponse.Value != null)
                {
                    ScoreSubjects = new ObservableCollection<ClassSubject>(csResponse.Value);
                }

                ShowScoreForm = true;
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យមុខវិជ្ជា: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
            }
        }

        [RelayCommand]
        private async Task SaveScoreAsync()
        {
            if (_selectedStudentForScore == null) return;

            if (SelectedScoreExam == null)
            {
                _messageService.Show("សូមជ្រើសរើសប្រឡង!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (SelectedScoreSubject == null)
            {
                _messageService.Show("សូមជ្រើសរើសមុខវិជ្ជា!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (ScoreAmount < 0)
            {
                _messageService.Show("សូមបញ្ចូលពិន្ទុត្រឹមត្រូវ!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                return;
            }

            if (!_studentIdToStudentClassId.TryGetValue(_selectedStudentForScore.Id, out int studentClassId))
            {
                _messageService.Show("មិនអាចកំណត់ StudentClassID បានទេ!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                return;
            }

            IsSavingScore = true;

            try
            {
                Assessment score = new()
                {
                    TotalScore = ScoreAmount,
                    ExamId = SelectedScoreExam.Id,
                    StudentClassId = studentClassId,
                    ClassSubjectId = SelectedScoreSubject.Id
                };

                var response = await _scoreService.InsertAsync(score);

                if (response.Status == Status.Success)
                {
                    _messageService.Show("បានបន្ថែមពិន្ទុដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);

                    ShowScoreForm = false;
                    _selectedStudentForScore = null;

                    if (SelectedExam != null)
                    {
                        await LoadScoresAsync(SelectedExam.Id);
                    }
                }
                else
                {
                    _messageService.Show(response.Message ?? "មានកំហុសបច្ចេកទេស​ក្នុងការរក្សាទុក!", "ERROR!", MessageButton.OK, MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសបច្ចេកទេស: {ex.Message}", "ERROR!", MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsSavingScore = false;
            }
        }

        [RelayCommand]
        private void CancelScore()
        {
            ShowScoreForm = false;
            _selectedStudentForScore = null;
        }

        [RelayCommand]
        private async Task GenerateReportAsync()
        {
            if (Students.Count == 0)
            {
                _messageService.Show("មិនមានសិស្សក្នុងថ្នាក់ដើម្បីធ្វើរបាយការណ៍ទេ!", "ព័ត៌មាន", MessageButton.OK, MessageIcon.Information);
                return;
            }

            string report = $"=== របាយការណ៍ថ្នាក់: {ClassName} ===\n\n";
            report += $"ចំនួនសិស្សសរុប: {Students.Count} នាក់\n";
            report += $"ចំនួនប្រឡងសរុប: {Exams.Count} ដង\n\n";

            if (SelectedExam != null && HasScores)
            {
                var examScores = Scores.Where(s => s.ExamId == SelectedExam.Id).ToList();
                report += $"ប្រឡង: {SelectedExam.Name}\n";
                report += $"ចំនួនពិន្ទុ: {examScores.Count}\n";
                if (examScores.Count > 0)
                {
                    report += $"ពិន្ទុមធ្យម: {examScores.Average(s => (double)s.TotalScore):F2}\n";
                    report += $"ពិន្ទុខ្ពស់: {examScores.Max(s => s.TotalScore)}\n";
                    report += $"ពិន្ទុទាប: {examScores.Min(s => s.TotalScore)}\n\n";

                    report += "បញ្ជីពិន្ទុ:\n";
                    foreach (Assessment score in examScores.OrderBy(s => s.StudentClass?.Student?.FullName))
                    {
                        string studentName = score.StudentClass?.Student?.FullName ?? "មិនស្គាល់";
                        string subjectName = score.ClassSubject?.Subject?.KhmerName ?? "មិនស្គាល់";
                        report += $"- {studentName} | {subjectName}: {score.TotalScore}\n";
                    }
                }
            }
            else
            {
                report += "សូមជ្រើសរើសប្រឡងដើម្បីមើលពិន្ទុលម្អិត។\n";
            }

            _messageService.Show(report, $"របាយការណ៍: {ClassName}", MessageButton.OK, MessageIcon.Information);
        }

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
                await _navigationService.NavigateAsync<ClassViewModel>();
            }
        }
    }
}
