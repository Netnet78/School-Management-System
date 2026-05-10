using School_Management.Application.Policies;
using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Models;

namespace School_Management.Application.Services
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
        }

        public async Task<bool> AuthorizeAsync(object? resource, OperatorMode operatorMode, params PermissionType[] permissions)
        {
            User? user = _userSessionService.CurrentUser;
            if (user == null) return false;

            foreach (var handler in _handlers)
            {
                if (await handler.HandleAsync(user, resource, operatorMode, permissions))
                    return true;
            }

            return false;
        }

        public async Task<bool> AuthorizeAsync(object? resource, PermissionType permission)
        {
            User? user = _userSessionService.CurrentUser;
            if (user == null) return false;

            foreach (var handler in _handlers)
            {
                if (await handler.HandleAsync(user, resource, OperatorMode.AND, permission))
                    return true;
            }

            return false;
        }
    }
}
