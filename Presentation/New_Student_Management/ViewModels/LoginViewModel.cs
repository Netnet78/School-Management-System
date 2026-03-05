using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Core.Models;
using School_Management.Application.Services;
using School_Management.Presentation.Shared.States;

namespace New_Student_Management.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserSessionState _userSession;
        private readonly IUserValidationService _userValidationService;

        public LoginViewModel(IUserSessionState userSessionService, IUserValidationService userValidationService)
        {
            _userSession = userSessionService;
            _userValidationService = userValidationService;
        }

        // MVVM Bindings
        [ObservableProperty]
        private string username = "";
        [ObservableProperty]
        private string password = "";
        public Action<bool>? LoginSucceeded { get; set; }

        // Login logics
        [RelayCommand]
        private async Task<bool> LoginAsync()
        {
            try
            {
                User? user = await _userValidationService.ValidateUserAsync(Username, Password);
                _userSession.SetUserSession(user.Username, user.Role.Name);
                LoginSucceeded?.Invoke(true);
                return true;
            }
            catch
            {
                LoginSucceeded?.Invoke(false);
                return false;
            }
        }

    }
}
