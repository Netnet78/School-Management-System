namespace SchoolManagement.Application.Features.Reports.Models
{
    public class StudentRosterFilter
    {
        public bool? IsActive { get; set; } = true;

        public string? SearchKeyword { get; set; }

        public int StartYear { get; set; } = DateTime.UtcNow.Year - 1;

        public int EndYear { get; set; } = DateTime.UtcNow.Year;

        public DateOnly? EnrollDateFrom { get; set; }

        public DateOnly? EnrollDateTo { get; set; }

        public DateOnly ReportDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}
