using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SchoolManagement.Presentation.Features.Dashboard.ViewModels
{
    public partial class DashboardViewModel : ObservableObject, IAsyncLoadable, IViewModel
    {
        private readonly IStudentService _studentService;
        private readonly IClassService _classService;
        private readonly IAttendanceService _attendanceService;
        private readonly IEmployeeService _employeeService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;

        public DashboardViewModel(
            IStudentService studentService,
            IClassService classService,
            IEmployeeService employeeService,
            IDispatcherService dispatcherService,
            IMessageService messageService,
            IAttendanceService attendanceService,
            IAuthorizationService authorizationService)
        {
            _studentService = studentService;
            _classService = classService;
            _employeeService = employeeService;
            _dispatcherService = dispatcherService;
            _messageService = messageService;
            _attendanceService = attendanceService;
            _authorizationService = authorizationService;

            for (int i = 2015; i <= DateTime.Now.Year; i++)
            {
                YearList.Add(i);
            }
        }

        [ObservableProperty]
        private int _totalStudents;
        [ObservableProperty]
        private int _totalClasses;
        [ObservableProperty]
        private int _totalTeachers;
        [ObservableProperty]
        private int _totalStayInStudents;
        [ObservableProperty]
        private int _fromYear = DateTime.Now.Year - 3;
        [ObservableProperty]
        private int _toYear = DateTime.Now.Year;
        [ObservableProperty]
        private List<int> _yearList = new();

        [ObservableProperty]
        private ChartObservableModel _studentsPerClassChart = new();

        [ObservableProperty]
        private ChartObservableModel _genderChart = new();
        [ObservableProperty]
        private string _maleCount = "";
        [ObservableProperty]
        private string _femaleCount = "";

        [ObservableProperty]
        private ChartObservableModel _attendanceChart = new();
        [ObservableProperty]
        private string _absentCount = "";
        [ObservableProperty]
        private string _lateCount = "";
        [ObservableProperty]
        private string _excusedCount = "";
        [ObservableProperty]
        private string _presentCount = "";
        [ObservableProperty]
        private bool _isHeadTeacherOrAdmin = false;

        [ObservableProperty]
        private ObservableCollection<Student> _recentStudents = new();

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            await LoadAsync();
        }

        private async Task LoadTotalStudents()
        {
            List<FilterCondition<Student>> filters =
            [
                new(s => s.Candidate.CreatedAt, FilterOperator.GreaterThanOrEqual, new DateTime(FromYear, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                new(s => s.Candidate.CreatedAt, FilterOperator.LessThanOrEqual, new DateTime(ToYear + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            ];

            ReturnResponse<int> studentCountResponse = await _studentService.GetAllCountAsync(1, int.MaxValue, filters);

            if (!EnsureSuccess(studentCountResponse, "Failed to load total students"))
                return;

            TotalStudents = studentCountResponse.Value;
        }

        private async Task LoadTotalClasses()
        {
            List<FilterCondition<Class>> filters =
                [
                    new(c => c.Generation.AcademicStartYear, FilterOperator.GreaterThanOrEqual, FromYear),
                    new(c => c.Generation.AcademicStartYear, FilterOperator.LessThanOrEqual, ToYear)
                ];

            filters = FiltersByPermission(filters);

            ReturnResponse<int> classCountReponse = await _classService.GetAllCountAsync(1, null, filters);

            if (!EnsureSuccess(classCountReponse, "Failed to load total classes"))
                return;

            TotalClasses = classCountReponse.Value;
        }

        private List<FilterCondition<Class>> FiltersByPermission(List<FilterCondition<Class>> filters)
        {
            User? user = _authorizationService.CurrentUser;
            if (user == null)
            {
                return filters;
            }

            if (_authorizationService.UserIsAdmin)
            {
                return filters;
            }

            if (user?.IsHeadTeacher() == true)
            {
                int? departmentId = user.Employee?.DepartmentId;
                if (departmentId.HasValue)
                {
                    filters.Add(new(c => c.Generation.Department.Id, FilterOperator.Equals, departmentId.Value));
                }
            }
            else
            {
                int? teacherId = user?.EmployeeId;
                if (teacherId.HasValue)
                {
                    filters.Add(new(c => c.TeacherId, FilterOperator.Equals, teacherId.Value));
                }
            }

            return filters;
        }

        private async Task LoadTotalTeachers()
        {
            FilterCondition<Employee>[] filters = 
                [
                    new(e => e.Position, FilterOperator.Contains, "teacher"),
                    new(e => e.IsActive, FilterOperator.Equals, true)
                ];
            ReturnResponse<int> returnResponse = await _employeeService.GetAllCountAsync(1, null, filters);

            if (!EnsureSuccess(returnResponse, "Failed to load total teachers"))
                return;

            TotalTeachers = returnResponse.Value;
        }

        private async Task LoadTotalStayInStudents()
        {
            List<FilterCondition<Student>> filters =
            [
                new(s => s.Candidate.StayType, FilterOperator.Equals, StudentStayType.Inside),
                new(s => s.Candidate.CreatedAt, FilterOperator.GreaterThanOrEqual, new DateTime(FromYear, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                new(s => s.Candidate.CreatedAt, FilterOperator.LessThanOrEqual, new DateTime(ToYear + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            ];

            ReturnResponse<int> studentCountResponse = await _studentService.GetAllCountAsync(1, int.MaxValue, filters);

            if (!EnsureSuccess(studentCountResponse, "Failed to load stay in students"))
                return;

            TotalStayInStudents = studentCountResponse.Value;
        }

        private async Task LoadStudentsPerClassChart()
        {
            ReturnResponse<IEnumerable<ClassStudentCountDto>> response = 
                await _classService.GetStudentCountPerClassAsync(FromYear, ToYear);

            if (!EnsureSuccess(response, "Failed to load students"))
                return;

            IEnumerable<ClassStudentCountDto> data = response.Value ?? [];

            int[] values = data.Select(x => x.Count).ToArray();
            string[] labels = data.Select(x => x.ClassName).ToArray();

            await _dispatcherService.InvokeAsync(() =>
            {
                StudentsPerClassChart.Series.Clear();

                StudentsPerClassChart.Series.Add(
                    new ColumnSeries<int>
                    {
                        Values = values
                    }
                );

                StudentsPerClassChart.XAxes =
                [
                    new Axis
                    {
                        Labels = labels
                    }
                ];

                StudentsPerClassChart.YAxes =
                [
                    new Axis
                    {
                        MinLimit = 0
                    }
                ];
            });
        }

        private async Task LoadAttendanceChart()
        {
            ReturnResponse authorizedResponse = await _authorizationService.AuthorizeAsync(
                null, PermissionType.ManageAttendances);

            if (authorizedResponse.Status != Status.Success)
            {
                _messageService.Show("អ្នកមិនអាចអានទិន្នន័យវត្តមានសិស្សបានទេ", "ឈប់សិន!", icon: MessageIcon.Hand);
                return;
            }

            ReturnResponse<int> lateCount = await _attendanceService.GetLateStudentCountToday();

            ReturnResponse<int> absentCount = await _attendanceService.GetAbsentStudentCountToday();

            ReturnResponse<int> excusedCount = await _attendanceService.GetExcusedStudentCountToday();

            ReturnResponse<int> presentCount = await _attendanceService.GetPresentStudentCountToday();

            await _dispatcherService.InvokeAsync(() =>
            {
                AttendanceChart.Series.Clear();

                AttendanceChart.Series.Add(new PieSeries<int>
                {
                    Values = [lateCount.Value],
                    Name = "Late",
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    Fill = new SolidColorPaint(SKColor.Parse("#FFB52C"))
                });

                AttendanceChart.Series.Add(new PieSeries<int>
                {
                    Values = [absentCount.Value],
                    Name = "Absent",
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    Fill = new SolidColorPaint(SKColor.Parse("#F03A17"))
                });

                AttendanceChart.Series.Add(new PieSeries<int>
                {
                    Values = [excusedCount.Value],
                    Name = "Excused",
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    Fill = new SolidColorPaint(SKColor.Parse("#F155E2"))
                });

                AttendanceChart.Series.Add(new PieSeries<int>
                {
                    Values = [presentCount.Value],
                    Name = "Present",
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    Fill = new SolidColorPaint(SKColor.Parse("#07DF03"))
                });

                int excused = excusedCount.Value;
                int present = presentCount.Value;
                int absent = absentCount.Value;
                int late = lateCount.Value;

                int total = excused + present + absent + late;
                double excusedPercent = (double)excused / total * 100;
                double presentPercent = (double)present / total * 100;
                double absentPercent = (double)absent / total * 100;
                double latePercent = (double)late / total * 100;

                ExcusedCount = $"ច្បាប់ (Excused): {excused} នាក់ ({excusedPercent:F0}%)";
                PresentCount = $"វត្តមាន (Present): {present} នាក់ ({presentPercent:F0}%)";
                AbsentCount = $"អត់ច្បាប់ (Absent): {absent} នាក់ ({absentPercent:F0}%)";
                LateCount = $"យឺត (Late): {late} នាក់ ({latePercent:F0}%)";
            });
        }

        private async Task LoadGenderChart()
        {
            List<FilterCondition<Student>> femaleFilters =
            [
                new(s => s.Candidate.Gender, FilterOperator.Equals, Gender.Female),
                new(s => s.Candidate.CreatedAt, FilterOperator.GreaterThanOrEqual, new DateTime(FromYear, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                new(s => s.Candidate.CreatedAt, FilterOperator.LessThanOrEqual, new DateTime(ToYear + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            ];

            var femaleCount = await _studentService.GetAllCountAsync(1, int.MaxValue, femaleFilters);

            if (!EnsureSuccess(femaleCount, "Failed to load female student count"))
                return;

            List<FilterCondition<Student>> maleFilters =
            [
                new(s => s.Candidate.Gender, FilterOperator.Equals, Gender.Male),
                new(s => s.Candidate.CreatedAt, FilterOperator.GreaterThanOrEqual, new DateTime(FromYear, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                new(s => s.Candidate.CreatedAt, FilterOperator.LessThanOrEqual, new DateTime(ToYear + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            ];

            var maleCount = await _studentService.GetAllCountAsync(1, int.MaxValue, maleFilters);

            if (!EnsureSuccess(maleCount, "Failed to load male student count"))
                return;

            await _dispatcherService.InvokeAsync(() =>
            {
                GenderChart.Series.Clear();

                GenderChart.Series.Add(new PieSeries<int>
                {
                    Values = [femaleCount.Value],
                    Name = "Female",
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    Fill = new SolidColorPaint(SKColor.Parse("#FF85C1"))
                });

                GenderChart.Series.Add(new PieSeries<int>
                {
                    Values = [maleCount.Value],
                    Name = "Male",
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    Fill = new SolidColorPaint(SKColor.Parse("#0082C9"))
                });

                int total = (maleCount.Value) + (femaleCount.Value);
                int totalMale = maleCount.Value;
                int totalFemale = femaleCount.Value;
                double malePercent = (double)totalMale / total * 100;
                double femalePercent = (double)totalFemale / total * 100;

                MaleCount = $"ប្រុស (Male): {maleCount.Value} នាក់ ({malePercent:F0}%)";
                FemaleCount = $"ស្រី (Female): {femaleCount.Value} នាក់ ({femalePercent:F0}%)";
            });
        }

        private async Task LoadNewDailyStudents()
        {
            User? user = _authorizationService.CurrentUser;

            if (user == null)
            {
                _messageService.Show("សូមធ្វើការ log in ដើម្បីចូលប្រើប្រាស់កម្មវិធី!", "ឈប់សិន!", MessageButton.OK, MessageIcon.Exclamation);
                return;
            }

            DateTime now = DateTime.UtcNow;
            DateTime today = new(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

            List<FilterCondition<Student>> filters = 
            [
                new(s => s.CreatedAt, FilterOperator.GreaterThanOrEqual, today),
            ];

            ReturnResponse<IEnumerable<Student>> students = await _studentService.GetAllAsync(1, null, filters, null);

            if (students.Status != Status.Success || students.Value == null)
            {
                _messageService.Show(students.Message, "ឈប់សិន!", icon: MessageIcon.Exclamation);
                return;
            }

            RecentStudents.Clear();
            foreach (Student student in students.Value)
            {
                RecentStudents.Add(student);    
            }
        }

        private bool EnsureSuccess<T>(ReturnResponse<T> response, string title)
        {
            if (response.Status == Status.Success)
                return true;

            if (response.Status == Status.Failed)
            {
                _messageService.Show(
                    response.Message ?? "Unknown error occurred",
                    title,
                    MessageButton.OK,
                    MessageIcon.Error
                );
            }

            return false;
        }

        partial void OnFromYearChanged(int oldValue, int newValue)
        {
            if (newValue < 1 || newValue > 9999)
                return;

            if (newValue > ToYear)
            {
                FromYear = oldValue;
                return;
            }
        }

        partial void OnToYearChanged(int oldValue, int newValue)
        {
            if (newValue < 1 || newValue > 9999)
                return;

            if (newValue < FromYear)
            {
                ToYear = oldValue;
                return;
            }
        }

        public async Task LoadAsync()
        {
            try
            {
                User? currentUser = _authorizationService.CurrentUser;
                IsHeadTeacherOrAdmin = _authorizationService.UserIsAdmin
                    || currentUser?.IsHeadTeacher() == true;
                await LoadTotalStudents();
                await LoadTotalStayInStudents();
                await LoadTotalTeachers();
                await LoadTotalClasses();
                await LoadStudentsPerClassChart();
                await LoadGenderChart();
                await LoadAttendanceChart();
                await LoadNewDailyStudents();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _messageService.Show(
                    $"For some reason, the data can't be fetched\n{ex.Message}", 
                    "Error", 
                    MessageButton.OK, MessageIcon.Error);
            }
        }
    }
}

