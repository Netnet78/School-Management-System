namespace SchoolManagement.Application.Features.Reports.Models
{
    public class AttendanceReportFilter
    {
        public List<int>? ClassIds { get; set; }

        public int? Month { get; set; }

        public int? Year { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string? SearchKeyword { get; set; }
    }
}
