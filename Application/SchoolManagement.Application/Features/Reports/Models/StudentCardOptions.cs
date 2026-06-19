namespace SchoolManagement.Application.Features.Reports.Models
{
    public class StudentCardOptions
    {
        public string PrincipalName { get; set; } = string.Empty;
        public string SignaturePath { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; } = DateTime.Now;
    }
}
