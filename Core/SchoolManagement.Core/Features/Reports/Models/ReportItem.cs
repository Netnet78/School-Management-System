namespace SchoolManagement.Core.Features.Reports.Models
{
    public class ReportItem
    {
        public int XPos { get; set; }
        public int YPos { get; set; }
        public object? Value { get; set; }
        public float FontSize { get; set; } = 10;
        public bool IsBold { get; set; }
        public string? FontColor { get; set; }
        public string? FontFamily { get; set; }
    }

    public class ReportItem<T> : ReportItem
    {
        public new T? Value { get; set; }
    }
}
