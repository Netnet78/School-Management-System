using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Core.Features.Auth.Enums;
using System.Diagnostics;

namespace CandidateManagement.ViewModels
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

                if (response.Status == Status.Success && 
                    !response.Value.IsValidRole(RoleType.Admin) &&
                    !response.Value.HasValidPermissions(OperatorMode.AND, PermissionType.ManageCandidates))
                {
                    _messageService.Show("អ្នកគ្មានសិទ្ធិក្នុងការប្រើប្រាស់កម្មវិធីនេះទេ! ប្រសិនបើអ្នកជឿជាក់ថាវាជាកំហុសបច្ចេកទេស សូមរាយការណ៍ទៅកាន់អ្នកគ្រប់គ្រងភ្លាម!",
                        "ឈប់ ឈប់ សិន!",
                        MessageButton.OK, MessageIcon.Hand);
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

                LoginSucceeded?.Invoke(false);
                return false;
            }
        }

    }
}
