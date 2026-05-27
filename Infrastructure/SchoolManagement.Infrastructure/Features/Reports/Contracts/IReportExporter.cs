using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Infrastructure.Features.Reports.Contracts
{
    public interface IReportExporter
    {
        string FormatName { get; }

        string FileExtension { get; }

        Task<byte[]> ExportAsync(ReportResult data, CancellationToken cancellationToken = default);

        Task ExportToFileAsync(ReportResult data, string filePath, CancellationToken cancellationToken = default);
    }
}
