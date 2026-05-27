using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Contracts
{
    public interface IReportGenerator
    {
        string ReportTypeKey { get; }

        object CreateDefaultFilter();

        Task<ReportResult> GenerateAsync(object filter, CancellationToken cancellationToken = default);
    }
}
