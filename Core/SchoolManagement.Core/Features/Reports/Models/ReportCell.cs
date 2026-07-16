using SchoolManagement.Core.Features.Reports.Enums;

namespace SchoolManagement.Core.Features.Reports.Models
{
    public class ReportCell
    {
        public object? Value { get; set; }

        public override string ToString() => Value?.ToString() ?? "";

        public double? FontSize { get; set; }

        public bool? IsBold { get; set; }

        public CellAlignment? Alignment { get; set; }

        public string? ForegroundColor { get; set; }

        public string? BackgroundColor { get; set; }

        public static implicit operator ReportCell(string? value) => new() { Value = value };

        public static implicit operator ReportCell(int value) => new() { Value = value };

        public static implicit operator ReportCell(double value) => new() { Value = value };

        public static implicit operator ReportCell(decimal value) => new() { Value = value };

        public static implicit operator ReportCell(float value) => new() { Value = value };

        public static implicit operator ReportCell(long value) => new() { Value = value };
    }
}
