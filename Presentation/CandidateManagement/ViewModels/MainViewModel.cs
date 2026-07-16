using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CandidateManagement.ViewModels
{
    public partial class MainViewModel : ObservableObject, IViewModel
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;

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
            INavigationService navigationService)
        {
            _userSessionService = userSessionService;
            _messageService = messageService;
            _navigationService = navigationService;

            _userSessionService.OnUserSessionChanged += OnUserSessionChanged;
            _navigationService.OnViewModelChanged += OnViewModelChanged;

            InitializeViewModelsAsync();
        }

        private async void OnUserSessionChanged(User? obj)
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
                await SetView(@new);
            }
        }

        private async void InitializeViewModelsAsync()
        {
            CurrentView = null;
            await ShowMainFormView();
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
        private async Task ShowMainFormView()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                await Task.Delay(1250);
                await _navigationService.NavigateAsync<MainFormViewModel>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ShowTableView()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                await Task.Delay(1250);
                await _navigationService.NavigateAsync<StudentViewModel>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ShowInsertView()
        {
            if (IsLoading) return;

            IsLoading = true;
            try 
            {
                await Task.Delay(1250);
                await _navigationService.NavigateAsync<InsertStudentViewModel>();
            }
            finally
            {
                IsLoading = false;
            }
            
        }

        [RelayCommand]
        private async Task ShowReportView()
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                await Task.Delay(1250);
                await _navigationService.NavigateAsync<ReportViewModel>();
            }
            finally
            {
                IsLoading = false;
            }
            
        }

        private async Task SetView(IViewModel viewModel)
        {
            if (viewModel.GetType() == CurrentView?.GetType()) return;

            try
            {
                CurrentView = viewModel;
            }
            catch (Exception ex)
            {
                _messageService.Show($"មិនអាចទៅរកផ្ទាំងមួយនេះបានទេ!: {ex.Message}", "អៃ... ចប់បណ្ដោយ....",
                    MessageButton.OK, MessageIcon.Error);
            }
        }
    }
}
