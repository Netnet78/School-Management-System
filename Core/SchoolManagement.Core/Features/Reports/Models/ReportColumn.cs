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
    }
}
