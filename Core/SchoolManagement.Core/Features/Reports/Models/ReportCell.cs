using SchoolManagement.Core.Features.Reports.Enums;

namespace SchoolManagement.Core.Features.Reports.Models
{
    public class ReportCell
    {
        public object? Value { get; init; }

        public override string ToString() => Value?.ToString() ?? "";

        public double? FontSize { get; init; }

        public bool? IsBold { get; init; }

        public CellAlignment? Alignment { get; init; }

        public string? ForegroundColor { get; init; }

        public string? BackgroundColor { get; init; }

        public static implicit operator ReportCell(string? value) => new() { Value = value };

        public static implicit operator ReportCell(int value) => new() { Value = value };

        public static implicit operator ReportCell(double value) => new() { Value = value };

        public static implicit operator ReportCell(decimal value) => new() { Value = value };

        public static implicit operator ReportCell(float value) => new() { Value = value };

        public static implicit operator ReportCell(long value) => new() { Value = value };
    }
}
