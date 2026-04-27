using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;
using System.Security;

namespace Attendance_Scanner.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserValidationService _userValidationService;
        private readonly IUserSessionService _userSessionService;
        private readonly IMessageService _messageService;

        public LoginViewModel(IUserValidationService userValidationService, IUserSessionService userSessionService, IMessageService messageService)
        {
            _userValidationService = userValidationService;
            _userSessionService = userSessionService;
            _messageService = messageService;
        }

        [ObservableProperty]
        private string username = string.Empty;

        [RelayCommand]
        public async Task LoginAsync(string securedPassword)
        {
            ReturnResponse<User> userResponse = await _userValidationService.ValidateUserAsync(Username, securedPassword);

            if (userResponse.Status == ReturnStatus.Failed)
            {
                _messageService.Show(userResponse.Message, "ខុសព័ត៌មាន!", icon: MessageIcon.Exclamation);
                return;
            }

            if (userResponse.Status == ReturnStatus.Rejected)
            {
                _messageService.Show(userResponse.Message, "ត្រជាក់ៗ! មួយៗ កុំលឿនពេក!", icon: MessageIcon.Hand);
                return;
            }

            await _userSessionService.SetSession(userResponse.Value!.Id);
        }
    }
}
