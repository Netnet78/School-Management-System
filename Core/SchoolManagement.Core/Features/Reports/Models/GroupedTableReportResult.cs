using SchoolManagement.Core.Features.Reports.Contracts;

namespace SchoolManagement.Core.Features.Reports.Models
{
    public record GroupedTableReportResult : ReportResult, ITableReportResult
    {
        public List<TableReportGroup> Groups { get; init; } = [];
        public List<ReportColumn> CommonColumns { get; init; } = [];
        public string TemplatePath { get; init; } = string.Empty;
    }
}
