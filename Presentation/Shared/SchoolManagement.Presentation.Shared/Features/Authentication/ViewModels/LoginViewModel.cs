using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Helpers;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;
using SchoolManagement.Core.Shared.Presentation.Contracts;
using System.Diagnostics;

namespace SchoolManagement.Presentation.Shared.ViewModels
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
        private string username = string.Empty;
        public Action<bool>? LoginSucceeded { get; set; }

        // Login logics
        [RelayCommand]
        private async Task<bool> LoginAsync(string password)
        {
            try
            {
                ReturnResponse<User> response = await _userValidationService.ValidateUserAsync(Username, password);

                string messageHeader = string.Empty;

                if (response.Status == Status.Failed) messageHeader = "ខុសព័ត៌មាន!";
                else if (response.Status == Status.Rejected) messageHeader = "ត្រជាក់ៗ! មួយៗ កុំលឿនពេក!";

                if (response.Value == null)
                {
                    _messageService.Show(response.Message, messageHeader,
                        MessageButton.OK, MessageIcon.Error);
                    return false;
                }

                await _userSessionService.SetSession(response.Value.Id);
                LoginSucceeded?.Invoke(true);
                return true;
            }
            catch (Exception ex)
            {
                _messageService.Show("There's something wrong when trying to login..." +
                    "\nThere are reasons that this error appears:" +
                    "\n1. Error establishing connection with the database" +
                    "\n2. Failed to initialize services" +
                    "\n3. Corrupted program files\n" +
                    "\n If you see this error, contact the administrator or the developer immediately.", "Critical Error",
                    MessageButton.OK, MessageIcon.Error);

                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.InnerException?.Message);
                Debug.WriteLine(ex.StackTrace);
                Debug.WriteLine(ex.Source);

                LoginSucceeded?.Invoke(false);
                return false;
            }
        }

    }
}
