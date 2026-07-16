using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Contracts
{
    public interface IReportGenerator
    {
        string ReportTypeKey { get; }

        object CreateDefaultRequest();

        Task<ReportResult> GenerateAsync(object filter, CancellationToken cancellationToken = default);
    }
}
