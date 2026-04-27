using CommunityToolkit.Mvvm.ComponentModel;
using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Presentation;

namespace School_Management.Presentation.Shared.Services
{
    public partial class LoadingService : ObservableObject, ILoadingService
    {
        [ObservableProperty]
        private LoadingState state = LoadingState.None;

        [ObservableProperty]
        private double progress;

        [ObservableProperty]
        private string message = string.Empty;

        [ObservableProperty]
        private bool isVisible;

        public void Hide()
        {
            IsVisible = false;
            State = LoadingState.None;
            Progress = 0.0;
        }

        public void ShowError(string? message = null)
        {
            State = LoadingState.Error;
            Message = message ?? "Error!";
            Progress = 0.0;
        }

        public void ShowLoading(string? message = null)
        {
            State = LoadingState.Loading;
            Message = message ?? "Loading...";
            Progress = 0.0;
            IsVisible = true;
        }

        public void ShowProgress(double value, string? message = null)
        {
            State = LoadingState.Progress;
            Progress = value;
            if (message != null)
                Message = message;

            IsVisible = true;
        }

        public void ShowSuccess(string? message = null)
        {
            State = LoadingState.Success;
            Message = message ?? "Success!";
        }
    }
}
