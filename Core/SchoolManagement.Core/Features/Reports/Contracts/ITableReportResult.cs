namespace SchoolManagement.Core.Features.Reports.Contracts
{
    /// <summary>
    /// Interface for defining any kind of report that shows tables when exported.
    /// </summary>
    public interface ITableReportResult
    {
        public string TemplatePath { get; init; }
    }
}
