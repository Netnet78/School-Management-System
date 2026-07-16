using SchoolManagement.Core.Features.Reports.Enums;

namespace SchoolManagement.Core.Features.Reports.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ReportTypeAttribute : Attribute
    {
        public string Key { get; init; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string DisplayNameKhmer { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconKind { get; set; } = "Document";
        public int SortOrder { get; set; }
        public ReportStyle ReportStyle { get; set; }
        public string[]? SupportedExportFormats { get; set; }
    }
}
