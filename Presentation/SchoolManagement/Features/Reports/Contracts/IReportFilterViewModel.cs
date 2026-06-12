using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.Contracts
{
    public interface IReportFilterViewModel : IViewModel
    {
        ReportTag ReportTypeKey { get; }

        event Action? FilterChanged;

        object GetFilterData();

        void ResetFilterData();
    }
}
