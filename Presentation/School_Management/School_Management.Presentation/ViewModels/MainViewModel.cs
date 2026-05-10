using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Application.Services;
using School_Management.Core.Enums;
using School_Management.Core.Interfaces;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;

namespace School_Management.Presentation.ViewModels
{
    public partial class MainViewModel : ObservableObject, IViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;
        private readonly IMessageService _messageService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IAuthorizationService _authorizationService;

        public event Action? OnExit;

        [ObservableProperty]
        private string _currentUsername = string.Empty;

        [ObservableProperty]
        private IViewModel? _currentViewModel = null;

        [ObservableProperty]
        private string _welcomeMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        // Permission for each roles
        [ObservableProperty]
        private bool _canAccessDashboard;
        [ObservableProperty]
        private bool _canAccessStudentListView;
        [ObservableProperty]
        private bool _canAccessClassView;
        [ObservableProperty]
        private bool _canAccessAttendanceView;
        [ObservableProperty]
        private bool _canAccessAnalyticsView;
        [ObservableProperty]
        private bool _canAccessEmployeeView;

        public MainViewModel(
            INavigationService navigationService,
            IUserSessionService userSessionService,
            IMessageService messageService,
            IDispatcherService dispatcherService,
            IAuthorizationService authorizationService)
        {
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            _messageService = messageService;
            _dispatcherService = dispatcherService;

            _userSessionService.OnUserSessionChanged += OnUserSessionChanged;
            _navigationService.OnViewModelChanged += OnViewModelChanged;

            _authorizationService = authorizationService;
        }

        [RelayCommand]
        private async Task ShowDashboardAsync()
        {
            await NavigateTo<DashboardViewModel>();
        }
        [RelayCommand]
        private async Task ShowAttendanceAsync()
        {
            await NavigateTo<AttendanceViewModel>();
        }
        [RelayCommand]
        private async Task ShowStudentListAsync()
        {
            await NavigateTo<StudentListViewModel>();
        }
        [RelayCommand]
        private async Task ShowClassAsync()
        {
            await NavigateTo<ClassViewModel>();
        }
        [RelayCommand]
        private async Task ShowReportAsync()
        {
            await NavigateTo<ReportViewModel>();
        }
        [RelayCommand]
        private async Task ShowEmployeeAsync()
        {
            await NavigateTo<EmployeeViewModel>();
        }

        private async Task NavigateTo<T>() where T : IViewModel
        {
            if (CurrentViewModel is T) return;

            IsLoading = true;
            try
            {
                await _navigationService.NavigateAsync<T>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnViewModelChanged(IViewModel? old, IViewModel current)
        {
            _dispatcherService.Invoke(() =>
            {
                CurrentViewModel = current;
            });
        }

        private async void OnUserSessionChanged(User? obj)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan noon = new(12, 0, 0);
            TimeSpan evening = new(17, 0, 0);
            string message;
            if (now < noon)
            {
                message = "អរុណសួស្ដី";
            }
            else if (now > noon && now < evening)
            {
                message = "សួស្ដី";
            }
            else
            {
                message = "សាយ័ន្តសួស្ដី";
            }

            if (obj != null)
            {
                CurrentUsername = obj.Username;
                WelcomeMessage = $"{message}, {obj.Username}!";
            }

            CanAccessDashboard = await HasPermission(PermissionType.ViewStudents);
            CanAccessStudentListView = await HasPermission(PermissionType.ViewStudents, PermissionType.EditStudents);
            CanAccessClassView = await HasPermission(
                PermissionType.ViewClasses);
            CanAccessAttendanceView = await HasPermission(PermissionType.ManageAttendances);
            CanAccessAnalyticsView = true;
            CanAccessEmployeeView = await HasPermission(PermissionType.ManageEmployees);
        }

        private async Task<bool> HasPermission(params PermissionType[] permissions)
        {
            return await _authorizationService.AuthorizeAsync(null, OperatorMode.OR, permissions);
        }

        [RelayCommand]
        private async Task ExitApplicationAsync()
        {
            MessageResult result = _messageService.Show("តើអ្នកប្រាកដទេថានឹងចាកចេញពីកម្មវិធីនេះ?", "ជម្រាបលា?", MessageButton.YesNo, MessageIcon.Question);

            if (result == MessageResult.Yes)
            {
                OnExit?.Invoke();
            }
        }

        public async Task LoadAsync()
        {
            CurrentViewModel = null;
            await ShowDashboardAsync();
        }
    }
}
