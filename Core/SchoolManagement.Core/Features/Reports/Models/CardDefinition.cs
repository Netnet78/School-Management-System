namespace SchoolManagement.Core.Features.Reports.Models
{
    public class CardDefinition
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string? TemplateFilePath { get; set; }
        public List<CardItem> Items { get; set; } = [];
    }
}
