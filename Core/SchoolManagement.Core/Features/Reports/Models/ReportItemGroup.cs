namespace SchoolManagement.Core.Features.Reports.Models
{
public class ReportItemGroup
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string? TemplateFileFilePath { get; set; }
    public List<CardItem> Items { get; set; } = [];
}
}
