using SchoolManagement.Core.Features.Reports.Enums;

namespace SchoolManagement.Core.Features.Reports.Models
{
    public class CardItem
    {
        public CardItem(object? value, int xPos, int yPos, float fontSize, bool isBold, string? fontColor, string? fontFamily, TextAlignment textAlignment, string? fieldName = null)
        {
            XPos = xPos;
            YPos = yPos;
            Value = value;
            FontSize = fontSize;
            IsBold = isBold;
            FontColor = fontColor;
            FontFamily = fontFamily;
            TextAlignment = textAlignment;
            FieldName = fieldName;
        }

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

    public class ReportItem<T> : CardItem
    {
        public new T? Value { get; set; }
    }
}
