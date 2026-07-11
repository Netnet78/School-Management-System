using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Core.Features.Exams.Models;
using SchoolManagement.Core.Features.Subjects.Models;
using SchoolManagement.Presentation.Shared.Features.Scores.Observables;
using SchoolManagement.Presentation.Shared.Features.Scores.Params;

namespace SchoolManagement.Presentation.Features.Scores.ViewModels
{
    public partial class ScoreViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly IStudentClassService _studentClassService;
        private readonly IExamService _examService;
        private readonly IAssessmentService _scoreService;
        private readonly IClassSubjectService _classSubjectService;
        private readonly ISubjectService _subjectService;
        private readonly INavigationService _navigationService;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IClassService _classService;

        private Class? _preSelectedClass;
        private List<int> _studentClassIds = [];
        private Dictionary<int, int> _studentIdToStudentClassId = [];
        private List<SubjectMapper> _cachedMappers = [];

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

        // ====== Mappers for the selected subject ======
        [ObservableProperty]
        private ObservableCollection<SubjectMapper> _mappers = [];

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
            ISubjectService subjectService,
            INavigationService navigationService,
            IMessageService messageService,
            IAuthorizationService authorizationService,
            IDispatcherService dispatcherService,
            IClassService classService)
        {
            _studentClassService = studentClassService;
            _examService = examService;
            _scoreService = scoreService;
            _classSubjectService = classSubjectService;
            _subjectService = subjectService;
            _navigationService = navigationService;
            _messageService = messageService;
            _authorizationService = authorizationService;
            _dispatcherService = dispatcherService;
            _classService = classService;
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
                _dispatcherService.Invoke(() =>
                {
                    _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                });
                return;
            }

            var authResult = await _authorizationService.AuthorizeAsync(null, PermissionType.ManageAssessments);
            _dispatcherService.Invoke(() =>
            {
                CanEditScores = authResult.Status == Status.Success;
            });

            await LoadExamsAsync();

            if (_preSelectedClass != null)
            {
                _dispatcherService.Invoke(() =>
                {
                    IsClassSelectionMode = false;
                    IsScoreEntryMode = true;
                    SelectedClass = _preSelectedClass;
                });
                await LoadSubjectsForClassAsync(_preSelectedClass.Id);
            }
            else
            {
                _dispatcherService.Invoke(() =>
                {
                    IsClassSelectionMode = true;
                    IsScoreEntryMode = false;
                });
                await LoadTeacherClassesAsync();
            }
        }

        // ====== Load teacher's classes ======

        private async Task LoadTeacherClassesAsync()
        {
            _dispatcherService.Invoke(() => IsLoadingClasses = true);

            try
            {
                User? user = _authorizationService.CurrentUser;
                if (user != null && user.IsAdmin())
                {
                    // Admin — load all classes
                    var allClassesResponse = await _classService.GetAllAsync(
                        filters: null,
                        page: 1,
                        pageSize: int.MaxValue,
                        orderBy: null,
                        includes: ["Grade", "Generation", "Generation.Department"]);

                    if (allClassesResponse.Status == Status.Success && allClassesResponse.Value != null)
                    {
                        _dispatcherService.Invoke(() =>
                        {
                            var classIds = new HashSet<int>();
                            Classes.Clear();
                            foreach (var c in allClassesResponse.Value)
                            {
                                if (classIds.Add(c.Id))
                                {
                                    Classes.Add(c);
                                }
                            }
                            Classes = new ObservableCollection<Class>(Classes.OrderBy(c => c.KhmerName));
                        });
                    }
                    return;
                }

                if (user != null && user.IsHeadTeacher())
                {
                    // Head teacher
                    var allClassesResponse = await _classService.GetAllAsync(
                        filters: [new(c => c.Generation.DepartmentId, FilterOperator.Equals, user.Employee?.DepartmentId)],
                        page: 1,
                        pageSize: int.MaxValue,
                        orderBy: null,
                        includes: ["Grade", "Generation", "Generation.Department"]);

                    if (allClassesResponse.Status == Status.Success && allClassesResponse.Value != null)
                    {
                        _dispatcherService.Invoke(() =>
                        {
                            var classIds = new HashSet<int>();
                            Classes.Clear();
                            foreach (var c in allClassesResponse.Value)
                            {
                                if (classIds.Add(c.Id))
                                {
                                    Classes.Add(c);
                                }
                            }
                            Classes = new ObservableCollection<Class>(Classes.OrderBy(c => c.KhmerName));
                        });
                        return;
                    }

                    // Teacher — load only classes they teach
                    var response = await _classSubjectService.GetAllAsync(
                        filters: [new FilterCondition<ClassSubject>(cs => cs.TeacherId, FilterOperator.Equals, user.EmployeeId)],
                        includes: ["Class.Grade", "Class.Generation.Department"]);

                    if (response.Status == Status.Success && response.Value != null)
                    {
                        _dispatcherService.Invoke(() =>
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
                        });
                    }

                    _dispatcherService.Invoke(() =>
                    {
                        Classes = new ObservableCollection<Class>(Classes.OrderBy(c => c.KhmerName));
                    });
                }
            }
            catch (Exception ex)
            {
                _dispatcherService.Invoke(() =>
                {
                    _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យថ្នាក់: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                });
            }
            finally
            {
                _dispatcherService.Invoke(() => IsLoadingClasses = false);
            }
        }

        async partial void OnSelectedClassChanged(Class? value)
        {
            if (value == null) return;

            _dispatcherService.Invoke(() =>
            {
                ClassName = value.GetKhmerName();
                ClassInfo = $"ថ្នាក់: {value.GetKhmerName()} | កម្រិត: {value.Grade?.KhmerName} | ជំនាន់: {value.Generation?.CohortNumber} | ផ្នែក: {value.Generation?.Department?.KhmerName}";

                IsClassSelectionMode = false;
                IsScoreEntryMode = true;
            });

            await LoadSubjectsForClassAsync(value.Id);
            _dispatcherService.Invoke(ClearScoreEntries);
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
                    _dispatcherService.Invoke(() =>
                    {
                        ScoreSubjects = new ObservableCollection<ClassSubject>(response.Value);
                    });
                }
            }
            catch (Exception ex)
            {
                _dispatcherService.Invoke(() =>
                {
                    _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យមុខវិជ្ជា: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                });
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
                    _dispatcherService.Invoke(() =>
                    {
                        Exams = new ObservableCollection<Exam>(response.Value);
                    });
                }
            }
            catch (Exception ex)
            {
                _dispatcherService.Invoke(() =>
                {
                    _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យប្រឡង: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                });
            }
        }

        // ====== Load mappers when subject changes ======

        async partial void OnSelectedScoreSubjectChanged(ClassSubject? value)
        {
            await LoadMappersAsync(value);
            await LoadScoreEntriesAsync();
        }

        async partial void OnSelectedExamChanged(Exam? value)
        {
            await LoadScoreEntriesAsync();
        }

        private async Task LoadMappersAsync(ClassSubject? classSubject)
        {
            _dispatcherService.Invoke(() =>
            {
                Mappers.Clear();
                _cachedMappers.Clear();
            });

            if (classSubject?.Subject == null) return;

            try
            {
                var mappers = await _subjectService.GetMappersForSubjectAsync(classSubject.Subject.Id);
                _dispatcherService.Invoke(() =>
                {
                    _cachedMappers = mappers.ToList();
                    Mappers = new ObservableCollection<SubjectMapper>(_cachedMappers);
                });
            }
            catch (Exception ex)
            {
                _dispatcherService.Invoke(() =>
                {
                    _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យសមាសភាគ: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                });
            }
        }

        // ====== Load bulk score entries ======

        private async Task LoadScoreEntriesAsync()
        {
            if (SelectedExam == null || SelectedScoreSubject == null || SelectedClass == null)
            {
                _dispatcherService.Invoke(() =>
                {
                    ScoreEntries.Clear();
                    HasScores = false;
                });
                return;
            }

            _dispatcherService.Invoke(() => IsLoadingScores = true);

            try
            {
                // 1. Load students in the class
                var scResponse = await _studentClassService.GetAllAsync(
                    filters: [new FilterCondition<StudentClass>(sc => sc.ClassId, FilterOperator.Equals, SelectedClass.Id)],
                    page: 1,
                    includes: ["Student.Candidate", "Student.Candidate.Skill"]);

                if (scResponse.Status != Status.Success || scResponse.Value == null)
                {
                    _dispatcherService.Invoke(() =>
                    {
                        ScoreEntries.Clear();
                        HasScores = false;
                    });
                    return;
                }

                List<StudentClass> studentClasses = scResponse.Value.ToList();
                var studentClassIds = studentClasses.Select(sc => sc.Id).ToList();
                var studentIdToStudentClassId = studentClasses
                    .Where(sc => sc.Student != null)
                    .ToDictionary(sc => sc.Student!.Id, sc => sc.Id);

                // 2. Load existing assessments for this exam + subject + class
                var existingResponse = await _scoreService.GetAllAsync(
                    filters: [
                        new FilterCondition<Assessment>(s => s.ExamId, FilterOperator.Equals, SelectedExam.Id),
                        new FilterCondition<Assessment>(s => s.ClassSubjectId, FilterOperator.Equals, SelectedScoreSubject.Id),
                        new FilterCondition<Assessment>(s => s.StudentClassId, FilterOperator.In, studentClassIds.Cast<object>())
                    ],
                    includes: ["Scores", "StudentClass.Student.Candidate"]);

                var existingByScId = new Dictionary<int, Assessment>();
                if (existingResponse.Status == Status.Success && existingResponse.Value != null)
                {
                    foreach (var a in existingResponse.Value)
                    {
                        existingByScId[a.StudentClassId] = a;
                    }
                }

                // 3. Build score entries with per-component scores
                _dispatcherService.Invoke(() =>
                {
                    _studentClassIds = studentClassIds;
                    _studentIdToStudentClassId = studentIdToStudentClassId;

                    ScoreEntries.Clear();
                    foreach (var sc in studentClasses)
                    {
                        if (sc.Student == null) continue;

                        existingByScId.TryGetValue(sc.Id, out Assessment? existing);

                        var entry = new StudentScoreEntry(sc.Student, sc.Id, existing);

                        foreach (var mapper in _cachedMappers)
                        {
                            decimal existingAmount = 0;
                            if (existing != null)
                            {
                                Score? score = existing.Scores.FirstOrDefault(s => s.MapperId == mapper.Id);
                                if (score != null)
                                    existingAmount = score.Amount;
                            }

                            entry.ComponentScores.Add(new ComponentScoreEntry(
                                mapper.Id,
                                mapper.ComponentId,
                                mapper.Component?.Name ?? string.Empty,
                                mapper.Component?.KhmerName ?? string.Empty,
                                existingAmount));
                        }

                        ScoreEntries.Add(entry);
                    }

                    HasScores = ScoreEntries.Count > 0;
                });
            }
            catch (Exception ex)
            {
                _dispatcherService.Invoke(() =>
                {
                    _messageService.Show($"មានកំហុសក្នុងការទាញទិន្នន័យពិន្ទុ: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                });
            }
            finally
            {
                _dispatcherService.Invoke(() => IsLoadingScores = false);
            }
        }

        // ====== Save All ======

        [RelayCommand]
        private async Task SaveAllAsync()
        {
            if (SelectedExam == null || SelectedScoreSubject == null)
            {
                _dispatcherService.Invoke(() =>
                {
                    _messageService.Show("សូមជ្រើសរើសប្រឡង និងមុខវិជ្ជា!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                });
                return;
            }

            if (_cachedMappers.Count == 0)
            {
                _dispatcherService.Invoke(() =>
                {
                    _messageService.Show("មុខវិជ្ជានេះមិនទាន់មានសមាសភាគទេ!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                });
                return;
            }

            // Validate against MaxScore per component
            foreach (var entry in ScoreEntries)
            {
                foreach (var comp in entry.ComponentScores)
                {
                    if (comp.ScoreAmount < 0)
                    {
                        _dispatcherService.Invoke(() =>
                        {
                            _messageService.Show($"ពិន្ទុរបស់ {entry.Student.FullName} មិនត្រឹមត្រូវទេ!", "ព័ត៌មានមិនគ្រប់", MessageButton.OK, MessageIcon.Information);
                        });
                        return;
                    }
                }
            }

            _dispatcherService.Invoke(() => IsSavingScores = true);

            try
            {
                var entries = ScoreEntries
                    .Where(e => e.ComponentScores.Count > 0)
                    .SelectMany(e => e.ComponentScores
                        .Where(c => c.ScoreAmount >= 0)
                        .Select(c => (e.StudentClassId, c.MapperId, c.ComponentId, c.ScoreAmount)));

                var response = await _scoreService.UpsertRangeAsync(SelectedExam.Id, SelectedScoreSubject.Id, entries);

                if (response.Status == Status.Success)
                {
                    _dispatcherService.Invoke(() =>
                    {
                        _messageService.Show("បានរក្សាទុកពិន្ទុទាំងអស់ដោយជោគជ័យ!", "ជោគជ័យ", MessageButton.OK, MessageIcon.Success);
                    });
                    await LoadScoreEntriesAsync();
                }
                else
                {
                    _dispatcherService.Invoke(() =>
                    {
                        _messageService.Show(response.Message ?? "មានកំហុសក្នុងការរក្សាទុក!", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                    });
                }
            }
            catch (Exception ex)
            {
                _dispatcherService.Invoke(() =>
                {
                    _messageService.Show($"មានកំហុសបច្ចេកទេស: {ex.Message}", "កំហុស!", MessageButton.OK, MessageIcon.Error);
                });
            }
            finally
            {
                _dispatcherService.Invoke(() => IsSavingScores = false);
            }
        }

        // ====== Change class (from score entry mode) ======

        [RelayCommand]
        private void ChangeClass()
        {
            _dispatcherService.Invoke(() =>
            {
                ClearScoreEntries();
                SelectedClass = null;
                SelectedExam = null;
                SelectedScoreSubject = null;
                Mappers.Clear();
                _cachedMappers.Clear();
                IsScoreEntryMode = false;
                IsClassSelectionMode = true;
            });
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

                int scoredCount = 0;
                foreach (var entry in ScoreEntries)
                {
                    if (entry.ComponentScores.Any(c => c.ScoreAmount >= 0))
                        scoredCount++;
                }

                report += $"ចំនួនពិន្ទុ: {scoredCount}\n";

                double totalSum = 0;
                int totalCount = 0;
                double maxAmount = 0;
                double minAmount = double.MaxValue;
                foreach (var entry in ScoreEntries)
                {
                    double studentTotal = 0;
                    bool hasScore = false;

                    foreach (var comp in entry.ComponentScores)
                    {
                        if (comp.ScoreAmount >= 0)
                        {
                            studentTotal += (double)comp.ScoreAmount;
                            hasScore = true;
                        }
                    }

                    if (hasScore)
                    {
                        totalSum += studentTotal;
                        totalCount++;
                        if (studentTotal > maxAmount) maxAmount = studentTotal;
                        if (studentTotal < minAmount) minAmount = studentTotal;
                    }
                }

                if (totalCount > 0)
                {
                    report += $"ពិន្ទុមធ្យម: {totalSum / totalCount:F2}\n";
                    report += $"ពិន្ទុខ្ពស់បំផុត: {maxAmount}\n";
                    report += $"ពិន្ទុទាបតិចបំផុត: {(minAmount == double.MaxValue ? 0 : minAmount)}\n\n";

                    report += "បញ្ជីពិន្ទុ:\n";
                    foreach (var entry in ScoreEntries.OrderBy(e => e.Student.FullName))
                    {
                        var parts = entry.ComponentScores
                            .Where(c => c.ScoreAmount >= 0)
                            .Select(c => $"{c.ComponentKhmerName}: {c.ScoreAmount}");
                        
                        if (parts.Any())
                        {
                            double studentTotal = entry.ComponentScores
                                .Where(c => c.ScoreAmount >= 0)
                                .Sum(c => (double)c.ScoreAmount);
                            report += $"- {entry.Student.FullName}: {string.Join(" | ", parts)} (សរុប: {studentTotal})\n";
                        }
                        else
                        {
                            report += $"- {entry.Student.FullName}: មិនមានពិន្ទុ\n";
                        }
                    }
                }
            }

            _messageService.Show(report, $"របាយការណ៍: {ClassName}", MessageButton.OK, MessageIcon.Information);
        }

        // ====== Navigation ======

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await _navigationService.NavigateAsync<ScoreViewModel>();
        }
    }
}