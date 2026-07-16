using SchoolManagement.Presentation.Shared.Components;
using System.Windows;

namespace SchoolManagement.Presentation.Shared.Services
{
    public class LoadingService : ILoadingService
    {
        private LoadingWindow? _window;

        public async Task ShowLoading(string? message = null)
        {
            await EnsureWindow();
            await Application.Current.Dispatcher.InvokeAsync(() => 
            {
                if (!_window!.IsVisible)
                    _window.Show();
                _window!.SetState(LoadingState.Loading, message ?? "Loading...");
            });
        }

        public async Task ShowProgress(double value, string? message = null)
        {
            await EnsureWindow();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (!_window!.IsVisible)
                    _window.Show();
                _window!.SetProgress(value, message);
            });
        }

        public async Task ShowSuccess(string? message = null)
        {
            await EnsureWindow();
            await Application.Current.Dispatcher.InvokeAsync(() => 
            {
                if (!_window!.IsVisible)
                    _window.Show();
                _window!.SetState(LoadingState.Success, message ?? "Success!");
            });
        }

        public async Task ShowError(string? message = null)
        {
            await EnsureWindow();
            await Application.Current.Dispatcher.InvokeAsync(() => 
            {
                if (!_window!.IsVisible)
                    _window.Show();
                _window!.SetState(LoadingState.Error, message ?? "Error!");
            });
        }

        public async Task Hide()
        {
            if (_window != null || _window?.IsVisible == true)
            {
                await Application.Current.Dispatcher.InvokeAsync(_window.Close);
            }
        }

        private async Task EnsureWindow()
        {
            if (_window == null || !_window.IsVisible)
            {
                await Application.Current.Dispatcher.Invoke(async() =>
                {
                    _window?.Close();
                    _window = new LoadingWindow
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    _window.Closed += (s, e) => _window = null;
                });
            }
        }
    }
}
