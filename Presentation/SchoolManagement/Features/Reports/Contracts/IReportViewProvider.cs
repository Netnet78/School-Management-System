using System.Windows.Controls;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.Contracts
{
    public interface IReportViewProvider
    {
        ReportTag ReportTypeKey { get; }

        IReportFilterViewModel? FilterViewModel { get; }

        UserControl PreviewView { get; }

        bool HasData { get; }

        string SummaryText { get; }

        int TotalCount { get; }

        bool CanExport(IReportExporter exporter);

        Task GenerateAsync(object filter, CancellationToken cancellationToken = default);

        Task ExportAsync(IReportExporter exporter, string filePath, CancellationToken cancellationToken = default);
    }
}
