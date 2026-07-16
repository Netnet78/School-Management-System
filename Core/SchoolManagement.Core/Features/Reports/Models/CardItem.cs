using SchoolManagement.Core.Features.Reports.Enums;

namespace SchoolManagement.Core.Features.Reports.Models
{
    public class CardItem
    {
        public CardItem() { }

        public int XPos { get; set; }
        public int YPos { get; set; }
        public object? Value { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public float FontSize { get; set; } = 10;
        public bool IsBold { get; set; }
        public string? FontColor { get; set; }
        public string? FontFamily { get; set; }
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;
        public string? FieldName { get; set; }
    }
}
