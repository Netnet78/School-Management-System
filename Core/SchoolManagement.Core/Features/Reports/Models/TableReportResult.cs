using SchoolManagement.Core.Features.Reports.Contracts;

namespace SchoolManagement.Core.Features.Reports.Models
{
    public record TableReportResult : ReportResult, ITableReportResult
    {
        public List<ReportColumn> Columns { get; init; } = [];
        public List<Dictionary<string, ReportCell>> Rows { get; init; } = [];
        public string TemplatePath { get; init; } = string.Empty;
    }
}
