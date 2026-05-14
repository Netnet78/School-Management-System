using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Measure;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView;
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
        private StudentFilterObservableModel _filters = new();
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
        private ObservableCollection<Student> _recentStudents = new();

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            await LoadAsync();
        }

        private async Task LoadTotalStudents()
        {
            ReturnResponse<int> studentCountResponse = await _studentService.GetStudentsCount(1, int.MaxValue, new()
            {
                FromDate = new DateTime(FromYear, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ToDate = new DateTime(ToYear + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });

            if (!EnsureSuccess(studentCountResponse, "Failed to load total students"))
                return;

            TotalStudents = studentCountResponse.Value;
        }

        private async Task LoadTotalClasses()
        {
            FilterCondition<Class>[] filters = 
                [
                    new(c => c.TeacherId ?? 0, FilterOperator.Equals, _authorizationService.CurrentUser.EmployeeId),
                    new(c => c.Generation.AcademicStartYear, FilterOperator.GreaterThanOrEqual, FromYear),
                    new(c => c.Generation.AcademicEndYear, FilterOperator.LessThanOrEqual, ToYear)
                ];

            ReturnResponse<int> classCountReponse = await _classService.GetAllCountAsync(1, null, filters);

            if (!EnsureSuccess(classCountReponse, "Failed to load total classes"))
                return;

            TotalClasses = classCountReponse.Value;
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
            StudentFilterOptions options = new()
            {
                StayType = StudentStayType.Inside,
                IsActive = true,
                FromDate = new DateTime(FromYear, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ToDate = new DateTime(ToYear + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            ReturnResponse<int> studentCountResponse = await _studentService.GetStudentsCount(1, int.MaxValue, options);

            if (!EnsureSuccess(studentCountResponse, "Failed to load stay in students"))
                return;

            TotalStayInStudents = studentCountResponse.Value;
        }

        private async Task LoadStudentsPerClassChart()
        {
            ReturnResponse<IEnumerable<ClassStudentCountDto>> response = await _classService.GetStudentCountPerClassAsync();

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
                _messageService.Show("??????????????????????????????????????", "????????????????????!", icon: MessageIcon.Hand);
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
                    Fill = Paint.Parse("#FFB52C")
                });

                AttendanceChart.Series.Add(new PieSeries<int>
                {
                    Values = [absentCount.Value],
                    Name = "Absent",
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    Fill = Paint.Parse("#F03A17")
                });

                AttendanceChart.Series.Add(new PieSeries<int>
                {
                    Values = [excusedCount.Value],
                    Name = "Excused",
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    Fill = Paint.Parse("#F155E2")
                });

                AttendanceChart.Series.Add(new PieSeries<int>
                {
                    Values = [presentCount.Value],
                    Name = "Present",
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    Fill = Paint.Parse("#07DF03")
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

                ExcusedCount = $"????????? (Excused): {excused} ???? ({excusedPercent:F0}%)";
                PresentCount = $"?????????? (Present): {present} ???? ({presentPercent:F0}%)";
                AbsentCount = $"????????? (Absent): {absent} ???? ({absentPercent:F0}%)";
                LateCount = $"??? (Late): {late} ???? ({latePercent:F0}%)";
            });
        }

        private async Task LoadGenderChart()
        {
            var femaleCount = await _studentService.GetStudentsCount(1, int.MaxValue, new()
            {
                Gender = Gender.Female,
                IsActive = true,
                FromDate = new DateTime(FromYear, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ToDate = new DateTime(ToYear + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            if (!EnsureSuccess(femaleCount, "Failed to load female student count"))
                return;

            var maleCount = await _studentService.GetStudentsCount(1, int.MaxValue, new()
            {
                Gender = Gender.Male,
                IsActive = true,
                FromDate = new DateTime(FromYear, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ToDate = new DateTime(ToYear + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });


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
                    Fill = Paint.Parse("#FF85C1")
                });

                GenderChart.Series.Add(new PieSeries<int>
                {
                    Values = [maleCount.Value],
                    Name = "Male",
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    Fill = Paint.Parse("#0082C9")
                });

                int total = (maleCount.Value) + (femaleCount.Value);
                int totalMale = maleCount.Value;
                int totalFemale = femaleCount.Value;
                double malePercent = (double)totalMale / total * 100;
                double femalePercent = (double)totalFemale / total * 100;

                MaleCount = $"???????? (Male): {maleCount.Value} ???? ({malePercent:F0}%)";
                FemaleCount = $"??????? (Female): {femaleCount.Value} ???? ({femalePercent:F0}%)";
            });
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
                await LoadTotalStudents();
                await LoadTotalStayInStudents();
                await LoadTotalTeachers();
                await LoadTotalClasses();
                await LoadStudentsPerClassChart();
                await LoadGenderChart();
                await LoadAttendanceChart();
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

