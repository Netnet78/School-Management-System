namespace SchoolManagement.Presentation.Features.Reports.Contracts;

public interface IReportOptionsViewModel
{
    event Action? OptionsChanged;

    object GetOptionsData();
}
