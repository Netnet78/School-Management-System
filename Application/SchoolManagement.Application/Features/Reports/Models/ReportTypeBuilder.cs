using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Models
{
    public class ReportTypeBuilder
    {
        public ReportTag Key { get; private set; }
        public string DisplayName { get; private set; } = string.Empty;
        public string DisplayNameKhmer { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string IconKind { get; private set; } = "Document";
        public int SortOrder { get; private set; }
        public bool IsCardReport { get; private set; }
        public bool IsGroupedReport { get; private set; }

        public ReportTypeBuilder WithKey(ReportTag key) { Key = key; return this; }
        public ReportTypeBuilder WithDisplayName(string english, string khmer) 
        { DisplayName = english; DisplayNameKhmer = khmer; return this; }
        public ReportTypeBuilder WithDescription(string description) { Description = description; return this; }
        public ReportTypeBuilder WithIcon(string icon) { IconKind = icon; return this; }
        public ReportTypeBuilder WithOrder(int order) { SortOrder = order; return this; }
        public ReportTypeBuilder AsCardReport() { IsCardReport = true; return this; }
        public ReportTypeBuilder AsGroupedReport() { IsGroupedReport = true; return this; }

        public ReportDefinition BuildDefinition() => new()
        {
            Key = Key,
            DisplayName = DisplayName,
            DisplayNameKhmer = DisplayNameKhmer,
            Description = Description,
            IconKind = IconKind,
            SortOrder = SortOrder,
        };
    }
}
