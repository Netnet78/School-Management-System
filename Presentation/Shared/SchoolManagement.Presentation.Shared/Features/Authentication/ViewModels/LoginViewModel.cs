using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SchoolManagement.Presentation.Shared.Features.Authentication.ViewModels
{
    public partial class LoginViewModel : ObservableObject, IViewModel
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IUserValidationService _userValidationService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;
        private readonly IDispatcherService _dispatcherService;

        public LoginViewModel(
            IUserSessionService userSessionService,
            IUserValidationService userValidationService,
            IMessageService messageService,
            INavigationService navigationService,
            IDispatcherService dispatcherService)
        {
            _userSessionService = userSessionService;
            _userValidationService = userValidationService;
            _messageService = messageService;
            _navigationService = navigationService;
            _dispatcherService = dispatcherService;
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
                bool result = await Task.Run(async () =>
                {
                    ReturnResponse<User> response = await _userValidationService.ValidateUserAsync(Username, password);

                    string messageHeader = string.Empty;

                    if (response.Status == Status.Failed) messageHeader = "ចូលមិនបានទេ!";
                    else if (response.Status == Status.Rejected) messageHeader = "មិនអនុញ្ញាតជាដាច់ខាត!";

                    _navigationService.ClearCache();

                    bool returned = await _dispatcherService.InvokeAsync(() =>
                    {
                        if (response.Value == null)
                        {
                            _messageService.Show(response.Message, messageHeader,
                                MessageButton.OK, MessageIcon.Error);
                            return false;
                        }

                        return true;
                    });

                    if (returned == true && response.Value != null)
                    {
                        await _userSessionService.SetSession(response.Value.Id).ConfigureAwait(false);

                        await _dispatcherService.InvokeAsync(() => LoginSucceeded?.Invoke(true));

                        return true;
                    }
                    else return false;
                });

                return result;
            }
            catch (Exception ex)
            {
                _messageService.Show("There's something wrong when trying to login..." +
                    "\nThere are reasons that this error appears:" +
                    "\n1. Error establishing connection with the database" +
                    "\n2. Failed to initialize services" +
                    "\n3. Corrupted program files\n" +
                    "\n If you see this error, contact the administrator or the developer immediately." +
                    $"\nError message:\n{ex.Message}", "Critical Error",
                    MessageButton.OK, MessageIcon.Error);

                LoginSucceeded?.Invoke(false);

                return false;
            }
        }

    }
}

