using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Attendances.Models;
using SchoolManagement.Core.Features.Reports.Attributes;
using SchoolManagement.Core.Features.Reports.Enums;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Generators
{
    [ReportType(Key = "attendance-report", DisplayName = "Attendance Report", DisplayNameKhmer = "របាយការណ៍វត្តមាន",
        Description = "View and export attendance summaries by student and class", IconKind = "ClipboardCheck", SortOrder = 2, ReportStyle = ReportStyle.Table,
        SupportedExportFormats = new[] { "Excel" })]
    public class AttendanceReportGenerator : IReportGenerator
    {
        private readonly IAttendanceRepository _attendanceRepository;

        public string ReportTypeKey => "attendance-report";

        public AttendanceReportGenerator(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public object CreateDefaultRequest() => new AttendanceReportFilter
        {
            DateFrom = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DateTo = DateTime.UtcNow,
        };

        public async Task<ReportResult> GenerateAsync(object filter, CancellationToken cancellationToken = default)
        {
            var attendanceFilter = (AttendanceReportFilter)filter;

            var filters = new List<FilterCondition<Attendance>>();

            if (attendanceFilter.DateFrom != null)
                filters.Add(new FilterCondition<Attendance>(a => a.AttendanceDateTime, FilterOperator.GreaterThanOrEqual, attendanceFilter.DateFrom.Value));

            if (attendanceFilter.DateTo != null)
                filters.Add(new FilterCondition<Attendance>(a => a.AttendanceDateTime, FilterOperator.LessThanOrEqual, attendanceFilter.DateTo.Value));

            if (!string.IsNullOrWhiteSpace(attendanceFilter.SearchKeyword))
                filters.Add(new FilterCondition<Attendance>(a => a.StudentClass.Student.Candidate.FullName, FilterOperator.Contains,
                    attendanceFilter.SearchKeyword));

            if (attendanceFilter.ClassId.HasValue)
                filters.Add(new FilterCondition<Attendance>(a => a.StudentClass.ClassId, FilterOperator.Equals, attendanceFilter.ClassId.Value));

            IEnumerable<Attendance> attendances = await _attendanceRepository.FindAsync(
                filters,
                page: null,
                pageSize: null,
                orderBy: new[] { new SortCriteria<Attendance>(a => a.AttendanceDateTime, OrderDirection.Ascending) },
                "StudentClass.Student.Candidate", "StudentClass.Class").ConfigureAwait(false);

            int totalCount = await _attendanceRepository.CountAsync(filters, attendanceFilter.Page, attendanceFilter.PageSize);

            var studentGroups = attendances
                .GroupBy(a => a.StudentClass?.StudentId)
                .ToList();

            var rows = new List<Dictionary<string, ReportCell>>();

            foreach (var group in studentGroups)
            {
                var first = group.First();
                var candidate = first.StudentClass?.Student?.Candidate;
                var @class = first.StudentClass?.Class;

                rows.Add(new Dictionary<string, ReportCell>
                {
                    ["studentName"] = candidate?.FullName,
                    ["latinName"] = candidate?.LatinFullName,
                    ["className"] = @class?.KhmerName,
                    ["totalDays"] = group.Count(),
                    ["present"] = group.Count(a => a.Status == AttendanceStatus.Present),
                    ["absent"] = group.Count(a => a.Status == AttendanceStatus.Absent),
                    ["late"] = group.Count(a => a.Status == AttendanceStatus.Late),
                    ["excused"] = group.Count(a => a.Status == AttendanceStatus.Excused || a.Status == AttendanceStatus.ExcusedLate),
                    ["attendanceRate"] = group.Count() > 0
                        ? Math.Round((double)group.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late) / group.Count() * 100, 1)
                        : 0.0,
                });
            }

            return new TableReportResult
            {
                ReportTag = "attendance-report",
                Title = "របាយការណ៍វត្តមានសិស្ស",
                SubTitle = "Attendance Report",
                GeneratedDate = DateTime.UtcNow,
                Columns =
                [
                    new() { Key = "studentName", Header = "Student Name", HeaderKhmer = "ឈ្មោះសិស្ស", Width = 200 },
                    new() { Key = "latinName", Header = "Latin Name", HeaderKhmer = "ឈ្មោះឡាតាំង", Width = 180 },
                    new() { Key = "className", Header = "Class", HeaderKhmer = "ថ្នាក់", Width = 150 },
                    new() { Key = "totalDays", Header = "Total Days", HeaderKhmer = "ចំនួនថ្ងៃសរុប", DataType = typeof(int), Width = 100 },
                    new() { Key = "present", Header = "Present", HeaderKhmer = "វត្តមាន", DataType = typeof(int), Width = 90 },
                    new() { Key = "absent", Header = "Absent", HeaderKhmer = "អវត្តមាន", DataType = typeof(int), Width = 90 },
                    new() { Key = "late", Header = "Late", HeaderKhmer = "យឺត", DataType = typeof(int), Width = 70 },
                    new() { Key = "excused", Header = "Excused", HeaderKhmer = "ច្បាប់", DataType = typeof(int), Width = 90 },
                    new() { Key = "attendanceRate", Header = "Rate (%)", HeaderKhmer = "អត្រា (%)", DataType = typeof(double), Format = "0.0", Width = 100 },
                ],
                Rows = rows,
                Summary = new Dictionary<string, object>
                {
                    ["__totalCount"] = totalCount,
                    ["សិស្សសរុប"] = studentGroups.Count,
                    ["ភាគរយនៃវត្តមានជាមធ្យម"] = rows.Count > 0
                        ? Math.Round(rows.Average(r => Convert.ToDouble(r.GetValueOrDefault("attendanceRate")?.Value ?? 0d)), 1)
                        : 0.0,
                }
            };
        }
    }
}
