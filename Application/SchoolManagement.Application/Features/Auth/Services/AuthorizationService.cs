using SchoolManagement.Application.Policies;
using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Application.Services
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

        public async Task<ReturnResponse> AuthorizeAsync(object? resource, OperatorMode operatorMode, params PermissionType[] permissions)
        {
            User? user = _userSessionService.CurrentUser;
            if (user == null) return new()
            {
                Message = "អ្នកមិនមានព័ត៌មាននៃអ្នកប្រើប្រាស់នៅក្នុងមូលដ្ឋានទិន្នន័យទេ!\nប្រសិនបើអ្នកគិតថា វាជាកំហុសបច្ចេកទេស" +
                "សូមធ្វើការទំនាក់ទំនងជាមួយនឹងអ្នកគ្រប់គ្រងភ្លាម!",
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
                        Message = "មិនអាចពិចារណាមុខងារដែលអ្នកមាននៅក្នុងមូលដ្ឋានទិន្នន័យនោះទេ! សូមព្យាយាមម្ដងទៀត។" +
                        $"\n Error Message: \n{ex.Message}"
                    };
                }

            }

            return new()
            {
                Status = Status.Rejected,
                Message = "អ្នកគ្មានសិទ្ធិចូលប្រើប្រាស់មុខងារនៅក្នុងកម្មវិធីនេះទេ!\nប្រសិនបើអ្នកគិតថា វាជាកំហុសបច្ចេកទេស" +
                "សូមធ្វើការទំនាក់ទំនងជាមួយនឹងអ្នកគ្រប់គ្រងភ្លាម!"
            };
        }

        public async Task<ReturnResponse> AuthorizeAsync(object? resource, PermissionType permission)
        {
            User? user = _userSessionService.CurrentUser;
            if (user == null) return new()
            {
                Message = "អ្នកមិនមានព័ត៌មាននៃអ្នកប្រើប្រាស់នៅក្នុងមូលដ្ឋានទិន្នន័យទេ!\nប្រសិនបើអ្នកគិតថា វាជាកំហុសបច្ចេកទេស" +
                "សូមធ្វើការទំនាក់ទំនងជាមួយនឹងអ្នកគ្រប់គ្រងភ្លាម!",
                Status = Status.Rejected,
            };

            foreach (var handler in _handlers)
            {
                try
                {
                    if (await handler.HandleAsync(user, resource, OperatorMode.AND, permission))
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
                        Message = "មិនអាចពិចារណាមុខងារដែលអ្នកមាននៅក្នុងមូលដ្ឋានទិន្នន័យនោះទេ! សូមព្យាយាមម្ដងទៀត។" +
                        $"\n Error Message: \n{ex.Message}"
                    };
                }
            }

            return new()
            {
                Status = Status.Rejected,
                Message = "អ្នកគ្មានសិទ្ធិចូលប្រើប្រាស់មុខងារនៅក្នុងកម្មវិធីនេះទេ!\nប្រសិនបើអ្នកគិតថា វាជាកំហុសបច្ចេកទេស" +
                "សូមធ្វើការទំនាក់ទំនងជាមួយនឹងអ្នកគ្រប់គ្រងភ្លាម!"
            };
        }
    }
}
