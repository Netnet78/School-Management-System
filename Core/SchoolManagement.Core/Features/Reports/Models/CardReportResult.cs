namespace SchoolManagement.Core.Features.Reports.Models
{
    public record CardReportResult : ReportResult
    {
        public List<CardItem>? CardItems { get; init; }
        public List<CardDefinition>? CardGroups { get; init; }
        public List<ReportColumn>? Columns { get; init; }
        public CardSheetLayout Layout { get; init; } = new();
    }
}
