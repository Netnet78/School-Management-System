namespace SchoolManagement.Application.Features.Reports.Models
{
    public class StudentCardFilter
    {
        public int? ClassId { get; set; }

        public string PrincipalName { get; set; } = string.Empty;

        public string SignaturePath { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string? SearchKeyword { get; set; }

        public bool ManualSelectEnabled { get; set; }

        public List<int>? SelectedStudentIds { get; set; }
    }
}
