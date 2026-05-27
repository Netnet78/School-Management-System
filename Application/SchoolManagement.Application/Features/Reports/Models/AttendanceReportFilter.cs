namespace SchoolManagement.Application.Features.Reports.Models
{
    public class AttendanceReportFilter
    {
        public int? ClassId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}
