using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Core.Models;
using School_Management.Core.Interfaces;
using School_Management.Presentation.Shared.Components;
using School_Management.Presentation.Shared.Enums;

namespace New_Student_Management.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IUserValidationService _userValidationService;
        private readonly IMessageService _messageService;

        public LoginViewModel(IUserSessionService userSessionService, IUserValidationService userValidationService, IMessageService messageService)
        {
            _userSessionService = userSessionService;
            _userValidationService = userValidationService;
            _messageService = messageService;
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

                if (user == null)
                {
                    _messageService.Show("Invalid username or password information, please try again!", "Wrong Credentials", 
                        System.Windows.MessageBoxButton.OK, MessageBoxIcon.Error);
                    return false;
                }

                await _userSessionService.SetSession(user);
                LoginSucceeded?.Invoke(true);
                return true;
            }
            catch
            {
                _messageService.Show("There's something wrong when trying to login..." +
                    "\nThere are reasons that this error appears:" +
                    "\n1. Error establishing connection with the database" +
                    "\n2. Failed to initialize services" +
                    "\n3. Corrupted program files\n" +
                    "\n If you see this error, contact the administrator or the developer immediately.", "Critical Error",
                    System.Windows.MessageBoxButton.OK, MessageBoxIcon.Error);

                LoginSucceeded?.Invoke(false);
                return false;
            }
        }

    }
}
