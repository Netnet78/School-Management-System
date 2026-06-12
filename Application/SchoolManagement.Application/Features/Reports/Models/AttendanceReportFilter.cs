namespace SchoolManagement.Application.Features.Reports.Models
{
    public class AttendanceReportFilter : IPagedFilter
    {
        public int? ClassId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string? SearchKeyword { get; set; }

        public int Page { get; set; } = 1;

        public int? PageSize { get; set; } = 10;
    }
}
