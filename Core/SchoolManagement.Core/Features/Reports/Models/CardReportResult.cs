namespace SchoolManagement.Core.Features.Reports.Models
{
    public record CardReportResult : ReportResult
    {
        public List<CardItem>? CardItems { get; init; }
        public List<ReportItemGroup>? CardGroups { get; init; }
        public CardSheetLayout Layout { get; init; } = new();
    }
}
