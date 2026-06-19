namespace SchoolManagement.Application.Features.Reports.Models
{
    public class StudentCardReportRequest
    {
        public StudentCardFilter Filter { get; set; } = new();
        public StudentCardOptions Options { get; set; } = new();
    }
}
