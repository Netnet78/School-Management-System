using SchoolManagement.Presentation.Shared.Features.Authentication.Views;

namespace SchoolManagement.Presentation.Shared.Services
{
    public class LoginService : ILoginService
    {
        private readonly INavigationService _navigationService;

        public LoginService(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public async Task<bool?> ShowLoginWindow<TViewModel>()
            where TViewModel : IViewModel
        {
            return await _navigationService.OpenDialog<LoginViewModel, LoginViewWindow>(typeof(TViewModel));
        }
    }
}
