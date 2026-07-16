namespace SchoolManagement.Application.Features.Reports.Models
{
    public class StudentCardFilter : IPagedFilter
    {
        public int? ClassId { get; set; }

        public string? SearchKeyword { get; set; }

        public bool ManualSelectEnabled { get; set; }

        public List<int>? SelectedStudentIds { get; set; }

        public int Page { get; set; } = 1;

        public int? PageSize { get; set; } = 10;
    }
}
