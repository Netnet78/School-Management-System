using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using School_Management.Application.Services;
using School_Management.Presentation.Shared.Helpers;
using System.ComponentModel;
using System.Windows;

namespace New_Student_Management.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserSessionService _userSession;
        private readonly Dictionary<Type, ObservableObject> _viewCache = new();

        [ObservableProperty]
        private bool isLoading;
        [ObservableProperty]
        private object? currentView;
        [ObservableProperty]
        private string username = "Guest";

        public Action? ExitAction { get; set; }

        public MainViewModel(IServiceProvider provider, IUserSessionService userSessionService)
        {
            // Default values
            _serviceProvider = provider;
            _userSession = userSessionService;

            // Set username from UserSession
            if (!String.IsNullOrEmpty(_userSession.Username))
            {
                Username = _userSession.Username;
            } 
            else
            {
                Username = "Guest";
            }
                _userSession.PropertyChanged += OnUserSessionChanged;

            // Default page
            _ = SetViewAsync<StudentViewModel>();
        }

        // MVVM Commands
        [RelayCommand]
        private void ExitApplication()
        {
            bool result = MessageBox.Show("Are you sure you want to exit the application?", "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            if (!result) return;

            ExitAction?.Invoke();
        }
        [RelayCommand]
        private async Task ShowTableViewAsync()
        {
            await SetViewAsync<StudentViewModel>();
        }
        [RelayCommand]
        private async Task ShowInsertViewAsync()
        {
            await SetViewAsync<InsertStudentViewModel>(); 
        }

        [RelayCommand]
        private async Task ShowReportViewAsync()
        {
            await SetViewAsync<ReportViewModel>();
        }

        // Utilities
        private async Task SetViewAsync<TViewModel>() where TViewModel : ObservableObject
        {
            IsLoading = true;

            try
            {
                if (!_viewCache.TryGetValue(typeof(TViewModel), out var vm))
                {
                    vm = _serviceProvider.GetRequiredService<TViewModel>();
                    _viewCache[typeof(TViewModel)] = vm;

                    // Load once only
                    if (vm is IAsyncLoadable loadable)
                    {
                        await loadable.LoadAsync();
                    }
                }

                // Switch view instantly
                CurrentView = vm;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                IsLoading = false;
            }
            finally
            {
                IsLoading = false;
            }
        }
        // User session change handler
        private void OnUserSessionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IUserSessionService.Username))
            {
                Username = _userSession.Username;
            }
        }
    }
}
