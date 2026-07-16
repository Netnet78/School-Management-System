using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Assets;
using SchoolManagement.Core.Features.Attendances.Models;
using SchoolManagement.Core.Features.Reports.Attributes;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;
using KhmerCalendar;

namespace SchoolManagement.Application.Features.Reports.Generators
{
    [ReportType(Key = "attendance-report", DisplayName = "Attendance Report", DisplayNameKhmer = "របាយការណ៍វត្តមាន",
        Description = "View and export attendance summaries by student and class", IconKind = "ClipboardCheck", SortOrder = 2, ReportStyle = ReportStyle.GroupedTable,
        SupportedExportFormats = new[] { "Excel" })]
    public class AttendanceReportGenerator : IReportGenerator
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IStudentClassRepository _studentClassRepository;
        private readonly string _templatePath = Path.Combine(ResourcePaths.Spreadsheets, "attendance_report.xlsx");

        public string ReportTypeKey => "attendance-report";

        public AttendanceReportGenerator(
            IAttendanceRepository attendanceRepository,
            IStudentClassRepository studentClassRepository)
        {
            _attendanceRepository = attendanceRepository;
            _studentClassRepository = studentClassRepository;
        }

        public object CreateDefaultRequest() => new AttendanceReportFilter
        {
            Month = DateTime.UtcNow.Month,
            Year = DateTime.UtcNow.Year,
        };

        public async Task<ReportResult> GenerateAsync(object filter, CancellationToken cancellationToken = default)
        {
            var attendanceFilter = (AttendanceReportFilter)filter;

            // Determine date range: Month/Year take precedence over DateFrom/DateTo
            DateTime dateFrom;
            DateTime dateTo;
            List<int> dayNumbers;

            if (attendanceFilter.Month.HasValue && attendanceFilter.Year.HasValue)
            {
                int year = attendanceFilter.Year.Value;
                int month = attendanceFilter.Month.Value;
                int daysInMonth = DateTime.DaysInMonth(year, month);
                dateFrom = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                dateTo = new DateTime(year, month, daysInMonth, 23, 59, 59, DateTimeKind.Utc);
                dayNumbers = Enumerable.Range(1, daysInMonth).ToList();
            }
            else
            {
                dateFrom = attendanceFilter.DateFrom ?? new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                dateTo = attendanceFilter.DateTo ?? DateTime.UtcNow;
                int totalDays = (int)(dateTo.Date - dateFrom.Date).TotalDays + 1;
                dayNumbers = Enumerable.Range(0, totalDays)
                    .Select(offset => dateFrom.AddDays(offset).Day)
                    .Distinct()
                    .ToList();
            }

            List<StudentClass> studentClasses;
            var classFilters = new List<FilterCondition<StudentClass>>();
            if (attendanceFilter.ClassIds?.Count > 0)
                classFilters.Add(new(sc => sc.ClassId, FilterOperator.In, attendanceFilter.ClassIds.Cast<object>()));

            var scResult = await _studentClassRepository.FindAsync(
                classFilters, includes: ["Student.Candidate", "Class", "Class.Generation", "Class.Generation.Department"]);
            studentClasses = scResult.ToList();

            // Apply search keyword filter on student name
            if (!string.IsNullOrWhiteSpace(attendanceFilter.SearchKeyword))
            {
                string keyword = attendanceFilter.SearchKeyword.Trim();
                studentClasses = studentClasses
                    .Where(sc => sc.Student?.FullName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }

            // Get all attendance records for these students in the date range
            var studentClassIds = studentClasses.Select(sc => sc.Id).ToList();
            var attendanceRecords = (await _attendanceRepository.FindAsync(
            [
                new FilterCondition<Attendance>(a => studentClassIds.Contains(a.StudentClassId)),
                new FilterCondition<Attendance>(a => a.AttendanceDateTime, FilterOperator.GreaterThanOrEqual, dateFrom),
                new FilterCondition<Attendance>(a => a.AttendanceDateTime, FilterOperator.LessThanOrEqual, dateTo),
            ],
                orderBy: new[] { new SortCriteria<Attendance>(a => a.AttendanceDateTime, OrderDirection.Ascending) }))
                .ToList();

            // Group attendance by StudentClassId then by day
            var attendanceByStudent = attendanceRecords
                .GroupBy(a => a.StudentClassId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Build dynamic day columns (shared across all groups)
            var columns = new List<ReportColumn>
            {
                new() { Key = "no", Header = "No.", HeaderKhmer = "ល.រ", DataType = typeof(int), Width = 50, Alignment = CellAlignment.Center },
                new() { Key = "studentName", Header = "Student Name", HeaderKhmer = "ឈ្មោះសិស្ស", Width = 200 },
                new() { Key = "gender", Header = "Gender", HeaderKhmer = "ភេទ", Width = 50, Alignment = CellAlignment.Center },
            };

            foreach (int day in dayNumbers)
            {
                DateTime currentDate = attendanceFilter.Month.HasValue && attendanceFilter.Year.HasValue
                    ? new DateTime(attendanceFilter.Year.Value, attendanceFilter.Month.Value,
                        Math.Min(day, DateTime.DaysInMonth(attendanceFilter.Year.Value, attendanceFilter.Month.Value)),
                        0, 0, 0, DateTimeKind.Utc)
                    : dateFrom.AddDays(day - dayNumbers[0]);

                string engDayName = currentDate.ToString("ddd");
                string khmerDayName = ((int)currentDate.DayOfWeek).UseKhmerDays();

                if (khmerDayName.Equals("ព្រហស្បតិ៍")) khmerDayName = "ព្រ-ហ";
                if (khmerDayName.Equals("អាទិត្យ")) khmerDayName = "អ-ទ";

                columns.Add(new ReportColumn
                {
                    Key = $"day_{day}",
                    Header = $"{engDayName}\n{day}",
                    HeaderKhmer = $"{khmerDayName}\n{day}",
                    Width = 44,
                    Alignment = CellAlignment.Center,
                    FontSize = 12,
                    IsBold = true
                });
            }

            columns.AddRange([
                new() { Key = "present", Header = "✓", HeaderKhmer = "វត្តមាន", DataType = typeof(int), Width = 60, Alignment = CellAlignment.Center },
                new() { Key = "late", Header = "L", HeaderKhmer = "យឺត", DataType = typeof(int), Width = 50, Alignment = CellAlignment.Center },
                new() { Key = "absent", Header = "A", HeaderKhmer = "អវត្តមាន", DataType = typeof(int), Width = 70, Alignment = CellAlignment.Center },
                new() { Key = "halfDay", Header = "H", HeaderKhmer = "ច្បាប់ចេញក្រៅ", DataType = typeof(int), Width = 100, Alignment = CellAlignment.Center },
                new() { Key = "excused", Header = "P", HeaderKhmer = "ច្បាប់", DataType = typeof(int), Width = 50, Alignment = CellAlignment.Center },
                new() { Key = "totalDays", Header = "Total", HeaderKhmer = "សរុប", DataType = typeof(int), Width = 60, Alignment = CellAlignment.Center },
                new() { Key = "attendanceRate", Header = "Rate (%)", HeaderKhmer = "អត្រា (%)", DataType = typeof(double), Format = "0.0", Width = 80, Alignment = CellAlignment.Center },
            ]);

            string baseTitle = "បញ្ជីស្រង់វត្តមានសិស្ស តាមជំនាញរបស់សាលាវិទ្យាល័យបច្ចេកទេស ដុនបូស្កូ ប៉ោយប៉ែត";

            var classGroups = studentClasses.GroupBy(sc => sc.ClassId);

            var groups = new List<TableReportGroup>();
            int totalStudentCount = 0;

            foreach (var classGroup in classGroups)
            {
                var sortedGroup = classGroup.OrderBy(sc =>
                    sc.Student?.Candidate?.FullName ?? sc.Student?.FullName ?? "");

                var firstSc = sortedGroup.First();
                var @class = firstSc.Class;
                string className = @class?.KhmerName ?? @class?.Name ?? $"Class {classGroup.Key}";

                if (attendanceFilter.Month != null)
                    baseTitle = $"បញ្ជីស្រង់វត្តមានសិស្ស{className} ប្រ​ចាំខែ{attendanceFilter.Month.Value.UseKhmerMonths()} " +
                    $"របស់សាលាវិទ្យាល័យបច្ចេកទេស ដុនបូស្កូ ប៉ោយប៉ែត";

                var groupRows = new List<Dictionary<string, ReportCell>>();
                int seqNo = 0;
                int classPresentTotal = 0, classLateTotal = 0, classAbsentTotal = 0;
                int classExcusedTotal = 0, classHalfDayTotal = 0, classTotalDays = 0;
                int femaleCount = 0;

                foreach (var sc in sortedGroup)
                {
                    seqNo++;
                    totalStudentCount++;
                    var student = sc.Student;
                    var candidate = student?.Candidate;
                    string genderCode = candidate?.Gender == Gender.Male ? "ប" : "ស";

                    var studentRecords = attendanceByStudent.GetValueOrDefault(sc.Id, []);

                    var dailyMap = studentRecords
                        .GroupBy(a => a.AttendanceDateTime.Day)
                        .ToDictionary(g => g.Key, g => g.OrderBy(a => a.AttendanceDateTime).Last().Status);

                    var row = new Dictionary<string, ReportCell>
                    {
                        ["no"] = seqNo,
                        ["studentName"] = candidate?.FullName ?? student?.FullName ?? "",
                        ["gender"] = genderCode,
                        ["latinName"] = candidate?.LatinFullName ?? "",
                        ["className"] = @class?.KhmerName ?? "",
                    };

                    foreach (int day in dayNumbers)
                    {
                        string? statusCode = null;
                        if (dailyMap.TryGetValue(day, out var status))
                        {
                            statusCode = status switch
                            {
                                AttendanceStatus.Present => "✓",
                                AttendanceStatus.Late => "L",
                                AttendanceStatus.ExcusedLate => "L",
                                AttendanceStatus.Absent => "A",
                                AttendanceStatus.Excused => "P",
                                AttendanceStatus.EarlyLeave => "H",
                                _ => null
                            };
                        }
                        row[$"day_{day}"] = statusCode;
                        row[$"day_{day}"].IsBold = true;
                    }

                    int presentCount = studentRecords.Count(r => r.Status == AttendanceStatus.Present);
                    int lateCount = studentRecords.Count(r => r.Status == AttendanceStatus.Late || r.Status == AttendanceStatus.ExcusedLate);
                    int absentCount = studentRecords.Count(r => r.Status == AttendanceStatus.Absent);
                    int excusedCount = studentRecords.Count(r => r.Status == AttendanceStatus.Excused);
                    int halfDayCount = studentRecords.Count(r => r.Status == AttendanceStatus.EarlyLeave);
                    int totalCount = studentRecords.Count;

                    row["present"] = presentCount;
                    row["late"] = lateCount;
                    row["absent"] = absentCount;
                    row["excused"] = excusedCount;
                    row["halfDay"] = halfDayCount;
                    row["totalDays"] = totalCount;
                    row["attendanceRate"] = totalCount > 0
                        ? Math.Round((double)(presentCount + lateCount) / totalCount * 100, 1)
                        : 0.0;

                    classPresentTotal += presentCount;
                    classLateTotal += lateCount;
                    classAbsentTotal += absentCount;
                    classExcusedTotal += excusedCount;
                    classHalfDayTotal += halfDayCount;
                    classTotalDays += totalCount;
                    if (candidate?.Gender == Gender.Female)
                        femaleCount++;

                    groupRows.Add(row);
                }

                int classStudentCount = classGroup.Count();
                double classAttendanceRate = classTotalDays > 0
                    ? Math.Round((double)(classPresentTotal + classLateTotal) / classTotalDays * 100, 1)
                    : 0.0;

                groups.Add(new TableReportGroup
                {
                    Title = $"{baseTitle}",
                    Name = @class?.Name ?? $"Class {classGroup.Key}",
                    KhmerName = @class?.KhmerName,
                    Columns = columns,
                    Rows = groupRows,
                    Summary = new Dictionary<string, object>
                    {
                        ["ចំនួនសិស្ស"] = classStudentCount,
                        ["ស្រី"] = femaleCount,
                        ["វត្តមាន"] = classPresentTotal,
                        ["យឺត"] = classLateTotal,
                        ["អវត្តមាន"] = classAbsentTotal,
                        ["ច្បាប់"] = classExcusedTotal,
                        ["ច្បាប់ចេញក្រៅ"] = classHalfDayTotal,
                        ["អត្រាវត្តមាន"] = $"{classAttendanceRate}%"
                    }
                });
            }

            return new GroupedTableReportResult
            {
                ReportTag = "attendance-report",
                Title = baseTitle,
                SubTitle = $"Attendance Report - {(attendanceFilter.ClassIds?.Count > 0 ? string.Join(", ", attendanceFilter.ClassIds) : "All Classes")}",
                GeneratedDate = DateTime.UtcNow,
                TemplatePath = _templatePath,
                Groups = groups,
                CommonColumns = columns,
                Summary = new Dictionary<string, object>
                {
                    ["__totalCount"] = totalStudentCount,
                    ["សិស្សសរុប"] = totalStudentCount,
                    ["ភាគរយនៃវត្តមានជាមធ្យម"] = totalStudentCount > 0
                        ? Math.Round(groups.Average(g => g.Rows.Count > 0
                            ? g.Rows.Average(r => Convert.ToDouble(r.GetValueOrDefault("attendanceRate")?.Value ?? 0d))
                            : 0d), 1)
                        : 0.0,
                }
            };
        }
    }
}
