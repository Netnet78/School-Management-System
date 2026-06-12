namespace SchoolManagement.Core.Features.Reports.Models
{
    /// <summary>
    /// A generic abstract class for defining report result.
    /// </summary>
    public abstract record ReportResult
    {
        /// <summary>
        /// Title of the report
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Subtitle of the report
        /// </summary>
        public string? SubTitle { get; init; }

        /// <summary>
        /// Date/time that the report was generated.
        /// The default value is set to <see cref="DateTime.Now"/>
        /// </summary>
        public DateTime GeneratedDate { get; init; } = DateTime.Now;

        /// <summary>
        /// A custom date/time that can be used to display in the report instead of the 
        /// <see cref="GeneratedDate"/>
        /// </summary>
        public DateTime? ReportDate { get; init; }

        /// <summary>
        /// Summary of the report, used to display information such as
        /// count, date/time, and etc.
        /// </summary>
        public Dictionary<string, object>? Summary { get; init; }

        /// <summary>
        /// The tag of the report that identifies the type of report
        /// </summary>
        public required ReportTag? ReportTag { get; set; }

    }
}
