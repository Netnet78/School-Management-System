namespace SchoolManagement.Presentation.Shared.Contracts
{
    public interface ILoadingService
    {
        Task ShowLoading(string? message = null);
        Task ShowProgress(double value, string? message = null);
        Task ShowSuccess(string? message = null);
        Task ShowError(string? message = null);
        Task Hide();
    }
}
