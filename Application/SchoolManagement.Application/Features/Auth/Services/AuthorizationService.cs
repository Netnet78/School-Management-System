namespace SchoolManagement.Application.Features.Auth.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IEnumerable<IAuthorizationHandler> _handlers;

        public AuthorizationService(
            IUserSessionService userSessionService,
            IEnumerable<IAuthorizationHandler> handlers)
        {
            _userSessionService = userSessionService;
            _handlers = handlers;

            RefreshCurrentUser();

            _userSessionService.OnUserSessionChanged += _ =>
            {
                RefreshCurrentUser();
            };
        }

        public User? CurrentUser { get; private set; }
        public bool UserIsAdmin { get; private set; } = false;

        private void RefreshCurrentUser()
        {
            CurrentUser = _userSessionService.CurrentUser;
            UserIsAdmin = CurrentUser?.IsAdmin() == true;
        }

        public async Task<ReturnResponse> AuthorizeAsync(object? resource, OperatorMode operatorMode, params PermissionType[] permissions)
        {
            User? user = _userSessionService.CurrentUser;
            if (user == null) return new()
            {
                Message = "ឈប់សិន!" +
                "មិនអនុញ្ញាតជាដាច់ខាត សូមធ្វើការចុះឈ្មោះជាអ្នកប្រើប្រាស់ត្រឹមត្រូវជាមុនសិន!",
                Status = Status.Rejected,
            };

            foreach (var handler in _handlers)
            {
                try
                {
                    if (await handler.HandleAsync(user, resource, operatorMode, permissions))
                        return new()
                        {
                            Status = Status.Success
                        };
                }
                catch (Exception ex)
                {
                    return new()
                    {
                        Status = Status.Failed,
                        Message = "មានបញ្ហាបច្ចេកទេស អំឡុងពេលដែលកំពុងត្រួតពិនិត្យការអនុញ្ញាត" +
                        $"\n Error Message: \n{ex.Message}"
                    };
                }

            }

            return new()
            {
                Status = Status.Rejected,
                Message = "ឈប់សិន!" +
                "អ្នកគ្មានការអនុញ្ញាត ក្នុងការបន្តទៅមុខទៀតទេ។"
            };
        }

        public async Task<ReturnResponse> AuthorizeAsync(object? resource, PermissionType permission)
        {
            return await AuthorizeAsync(resource, OperatorMode.AND, permission);
        }
    }
}

