using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Core.Models;
using School_Management.Infrastructure.Repositories;
using School_Management.Application.Services;

namespace New_Student_Management.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserSessionService _userSession;

        public LoginViewModel(IUserRepository userRepository, IUserSessionService userSessionService)
        {
            _userSession = userSessionService;
            _userRepository = userRepository;
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
            User? user = await _userRepository.ValidateUserAsync(Username, Password);

            if (user == null)
            {
                LoginSucceeded?.Invoke(false);
                return false;
            }

            _userSession.SetUserSession(user.Username, user.Role);
            LoginSucceeded?.Invoke(true);
            return true;
        }

    }
}
