using SchoolManagement.Core.Features.Reports.Enums;

namespace SchoolManagement.Core.Features.Reports.Models
{
    public class ReportColumn
    {
        public string Key { get; set; } = string.Empty;

        public string Header { get; set; } = string.Empty;

        public string? HeaderKhmer { get; set; }

        public Type DataType { get; set; } = typeof(string);

        public string? Format { get; set; }

        public double Width { get; set; } = 100;

        public double? FontSize { get; set; }

        public bool IsBold { get; set; }

        public CellAlignment Alignment { get; set; } = CellAlignment.Left;

        public string? ForegroundColor { get; set; }

        public string? BackgroundColor { get; set; }

        public string? HeaderGroup { get; set; }

        public double? HeaderFontSize { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}
