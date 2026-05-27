using SchoolManagement.Application.Features.Reports.Contracts;
using SchoolManagement.Application.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.Contracts
{
    public interface IReportComponentFactory
    {
        IReportGenerator CreateGenerator(ReportDefinition definition);
        IReportFilterViewModel CreateFilterViewModel(ReportDefinition definition);
    }
}
