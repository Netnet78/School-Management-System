using SchoolManagement.Application.Features.Reports.Models;

namespace SchoolManagement.Application.Features.Reports.Contracts
{
    public interface IReportRegistry
    {
        IReadOnlyList<ReportDefinition> GetAll();

        ReportDefinition? GetByKey(string key);
    }
}
