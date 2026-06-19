using System.Windows.Controls;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.Contracts
{
    /// <summary>
    /// Defines the contract for a provider that supplies report views, including filtering, preview, export, and
    /// summary information for a specific report type.
    /// </summary>
    /// <remarks>Implementations of this interface enable integration of custom report views with filtering,
    /// preview, and export capabilities. Each provider corresponds to a specific report type, identified by the
    /// ReportTypeKey property. The interface supports asynchronous generation and export of report data, and exposes
    /// properties for supported export formats, summary information, and data availability.</remarks>
    public interface IReportViewProvider
    {
        string ReportTypeKey { get; }

        IReportFilterViewModel? FilterViewModel { get; }

        IReportOptionsViewModel? OptionsViewModel { get; }

        /// <summary>
        /// Gets the list of supported export formats for the current instance.
        /// Currently this codebase supports 'PDF' and 'Excel' export.
        /// </summary>
        string[] SupportedExportFormats { get; }

        UserControl PreviewView { get; }

        bool HasData { get; }

        string SummaryText { get; }

        int TotalCount { get; }

        Task GenerateAsync(object filter, CancellationToken cancellationToken = default);

        Task ExportAsync(IReportExporter exporter, string filePath, CancellationToken cancellationToken = default);
    }
}
