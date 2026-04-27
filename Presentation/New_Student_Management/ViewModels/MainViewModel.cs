using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;

namespace New_Student_Management.ViewModels
{
    public partial class MainViewModel : ObservableObject, IViewModel
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IDispatcherService _dispatcherService;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private string _username = "Guest";

        public Action? ExitAction { get; set; }

        public MainViewModel(
            IMessageService messageService,
            IUserSessionService userSessionService,
            INavigationService navigationService,
            IDispatcherService dispatcherService)
        {
            _userSessionService = userSessionService;
            _messageService = messageService;
            _navigationService = navigationService;
            _dispatcherService = dispatcherService;

            _userSessionService.OnUserSessionChanged += OnUserSessionChanged;
            _navigationService.OnViewModelChanged += OnViewModelChanged;

            InitializeViewModelsAsync();
        }

        private void OnUserSessionChanged(User? obj)
        {
            // Set username from session
            User? user = obj;
            Username = user == null
                ? "Guest"
                : user.Username;
        }

        private async void OnViewModelChanged(IViewModel? old, IViewModel @new)
        {
            if (old != @new)
            {
                await _dispatcherService.InvokeAsync(async () =>
                {
                    await SetView(@new);
                });
            }
        }

        private async void InitializeViewModelsAsync()
        {
            CurrentView = null;
            await ShowTableView();
        }

        [RelayCommand]
        private async Task ExitApplication()
        {
            bool result = _messageService.Show(
                "Are you sure you want to exit the application?",
                "Confirm Exit",
                MessageButton.YesNo,
                MessageIcon.Question) == MessageResult.Yes;

            if (!result) return;

            ExitAction?.Invoke();
        }

        [RelayCommand]
        private async Task ShowTableView()
        {
            await _navigationService.NavigateAsync<StudentViewModel>();
        }

        [RelayCommand]
        private async Task ShowInsertView()
        {
            await _navigationService.NavigateAsync<InsertStudentViewModel>();
        }

        [RelayCommand]
        private async Task ShowReportView()
        {
            await _navigationService.NavigateAsync<ReportViewModel>();
        }

        private async Task SetView<T>() where T : IViewModel
        {
            if (typeof(T) == CurrentView?.GetType()) return;

            if (IsLoading) return;

            IsLoading = true;
            try
            {
                CurrentView = null;

                await Task.Delay(1250);

                CurrentView = _navigationService.CurrentViewModel;
            }
            catch (Exception ex)
            {
                _messageService.Show($"មិនអាចទៅរកផ្ទាំងមួយនេះបានទេ!: {ex.Message}", "អៃ... ចប់បណ្ដោយ....",
                    MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SetView(IViewModel viewModel)
        {
            if (viewModel.GetType() == CurrentView?.GetType()) return;

            if (IsLoading) return;

            IsLoading = true;
            try
            {
                CurrentView = null;

                await Task.Delay(1250);

                CurrentView = _navigationService.CurrentViewModel;
            }
            catch (Exception ex)
            {
                _messageService.Show($"មិនអាចទៅរកផ្ទាំងមួយនេះបានទេ!: {ex.Message}", "អៃ... ចប់បណ្ដោយ....",
                    MessageButton.OK, MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}