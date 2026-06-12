namespace SchoolManagement.Core.Features.Reports.Models
{
    public record TableReportGroup
    {
        /// <summary>
        /// Title of the table report
        /// </summary>
        public string Title { get; init; } = string.Empty;
        /// <summary>
        /// Name of the table report that can be used to identify the report section
        /// </summary>
        public string Name { get; init; } = string.Empty;
        /// <summary>
        /// Khmer name of the table report that can be used to identify the report section
        /// </summary>
        public string? KhmerName { get; init; }
        /// <summary>
        /// Columns of the table report
        /// </summary>
        public List<ReportColumn> Columns { get; init; } = [];
        /// <summary>
        /// Rows that contain data
        /// </summary>
        public List<Dictionary<string, ReportCell>> Rows { get; init; } = [];
        /// <summary>
        /// Data summarization of the table
        /// </summary>
        public Dictionary<string, object> Summary { get; init; } = [];
    }
}
