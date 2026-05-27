namespace SchoolManagement.Core.Features.Reports.Models
{
    public class ReportItemGroup
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<ReportItem> Items { get; set; } = [];
    }
}
