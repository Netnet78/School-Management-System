using SchoolManagement.Core.Features.Reports.Enums;

namespace SchoolManagement.Core.Features.Reports.Models
{
    public class ReportResult
    {
        public string Title { get; set; } = string.Empty;

        public string? SubTitle { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        public ReportLayout Layout { get; set; } = ReportLayout.Table;

        public List<ReportColumn> Columns { get; set; } = [];

        public List<Dictionary<string, object?>> Rows { get; set; } = [];

        public Dictionary<string, object>? Summary { get; set; }

        public List<ReportItem>? CardItems { get; set; }

        public List<ReportItemGroup>? CardGroups { get; set; }

        public string? TemplatePath { get; set; }
    }
}
