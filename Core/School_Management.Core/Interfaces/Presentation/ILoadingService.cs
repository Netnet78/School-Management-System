using School_Management.Core.Enums;

namespace School_Management.Core.Interfaces.Presentation
{
    public interface ILoadingService
    {
        string Message { get; }
        LoadingState State { get; }
        double Progress { get; }

        void ShowLoading(string? message = null);
        void ShowProgress(double value, string? message = null);
        void ShowSuccess(string? message = null);
        void ShowError(string? message = null);
        /// <summary>
        /// Hide the loading ui in the main window
        /// </summary>
        void Hide();
    }
}
