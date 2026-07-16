using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Infrastructure.Features.Reports.Contracts
{
    public interface IReportExporter
    {
        string FormatName { get; }

        string FileExtension { get; }

        Task<byte[]> ExportAsync<TReportResult>(TReportResult data, CancellationToken cancellationToken = default)
            where TReportResult : ReportResult;

        Task ExportToFileAsync<TReportResult>(TReportResult data, string filePath, CancellationToken cancellationToken = default)
            where TReportResult : ReportResult;
    }
}
