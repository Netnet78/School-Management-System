namespace SchoolManagement.Application.Features.Reports.Models
{
    public class ScoreReportFilter : IPagedFilter
    {
        public List<int> ClassIds { get; set; } = [];

        public int? SubjectId { get; set; }

        public int? ExamId { get; set; }

        public string? SearchKeyword { get; set; }

        public int Page { get; set; } = 1;

        public int? PageSize { get; set; } = 10;
    }
}
