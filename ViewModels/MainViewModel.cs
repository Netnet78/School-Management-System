using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Student_Management.Services;
using System.ComponentModel;
using System.Windows;

namespace Student_Management.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserSessionService _userSession;

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
            SetView<StudentViewModel>();
        }

        // MVVM Commands
        [RelayCommand]
        private void ExitApplication()
        {
            bool result = MessageBox.Show("Are you sure you want to exit the application?", "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            if (result == false) return;

            ExitAction?.Invoke();
        }
        [RelayCommand]
        private void ShowTableView()
        {
            SetView<StudentViewModel>();
        }
        [RelayCommand]
        private void ShowInsertView()
        { SetView<InsertStudentViewModel>(); }
        [RelayCommand]
        private void ShowReportView()
        { SetView<ReportViewModel>(); }

        // Utilities
        private void SetView<TViewModel>() where TViewModel : ObservableObject
        {
            CurrentView = _serviceProvider.GetRequiredService<TViewModel>();
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
