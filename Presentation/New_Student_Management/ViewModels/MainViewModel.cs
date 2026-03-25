using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Presentation.Shared.Helpers;
using System.Windows;
using School_Management.Core.Interfaces;
using School_Management.Core.Models;
using School_Management.Presentation.Shared.Components;
using School_Management.Presentation.Shared.Enums;

namespace New_Student_Management.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IMessageService _messageService;

        // Cache ViewModels as fields
        private readonly StudentViewModel _studentViewModel;
        private readonly InsertStudentViewModel _insertStudentViewModel;
        private readonly ReportViewModel _reportViewModel;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private object? currentView;

        [ObservableProperty]
        private string username = "Guest";

        public Action? ExitAction { get; set; }

        public MainViewModel(
            IMessageService messageService,
            IUserSessionService userSessionService,
            StudentViewModel studentViewModel,
            InsertStudentViewModel insertStudentViewModel,
            ReportViewModel reportViewModel)
        {
            _userSessionService = userSessionService;
            _messageService = messageService;

            // Assign injected ViewModels
            _studentViewModel = studentViewModel;
            _insertStudentViewModel = insertStudentViewModel;
            _reportViewModel = reportViewModel;

            // Set username from session
            User? user = _userSessionService.CurrentUser;
            Username = user == null
                ? "Guest"
                : user.Username;

            CurrentView = null;

            InitializeViewModelsAsync();
        }

        private async void InitializeViewModelsAsync()
        {
            try
            {
                IsLoading = true;

                if (_studentViewModel is IAsyncLoadable studentLoadable)
                    await studentLoadable.LoadAsync();

                if (_insertStudentViewModel is IAsyncLoadable insertLoadable)
                    await insertLoadable.LoadAsync();

                if (_reportViewModel is IAsyncLoadable reportLoadable)
                    await reportLoadable.LoadAsync();

                // Default page
                CurrentView = _insertStudentViewModel;
            }
            catch (Exception ex)
            {
                _messageService.Show($"Failed to initialize views: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ExitApplication()
        {
            bool result = _messageService.Show(
                "Are you sure you want to exit the application?",
                "Confirm Exit",
                MessageBoxButton.YesNo,
                MessageBoxIcon.Question) == MessageBoxResult.Yes;

            if (!result) return;

            ExitAction?.Invoke();
        }

        [RelayCommand]
        private void ShowTableView()
        {
            CurrentView = _studentViewModel;
        }

        [RelayCommand]
        private void ShowInsertView()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            CurrentView = _insertStudentViewModel;

            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"Switch time: {sw.ElapsedMilliseconds} ms");
        }

        [RelayCommand]
        private void ShowReportView()
        {
            CurrentView = _reportViewModel;
        }
    }
}