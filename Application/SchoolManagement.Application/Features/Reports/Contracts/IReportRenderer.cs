using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Contracts
{
    public interface IReportRenderer
    {
        bool CanRender(ReportResult result);

        Task<object> Render(ReportResult result);
    }
}
