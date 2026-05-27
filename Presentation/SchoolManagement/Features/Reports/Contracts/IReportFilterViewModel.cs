namespace SchoolManagement.Presentation.Features.Reports.Contracts
{
    public interface IReportFilterViewModel : IViewModel
    {
        string ReportTypeKey { get; }

        event Action? FilterChanged;

        object GetFilterData();
    }
}
