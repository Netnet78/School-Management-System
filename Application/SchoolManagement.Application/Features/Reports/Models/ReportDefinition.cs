namespace SchoolManagement.Application.Features.Reports.Models
{
    public class ReportDefinition
    {
        public string Key { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string DisplayNameKhmer { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IconKind { get; set; } = "Document";

        public Type GeneratorType { get; set; } = null!;

        public Type FilterViewModelType { get; set; } = null!;

        public int SortOrder { get; set; }

        public string? TemplatePath { get; set; }
    }
}
