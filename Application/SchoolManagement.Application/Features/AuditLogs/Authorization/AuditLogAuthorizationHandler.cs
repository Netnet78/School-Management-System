namespace SchoolManagement.Application.Features.AuditLogs.Authorization
{
    public class AuditLogAuthorizationHandler : IAuthorizationHandler
    {
        public async Task<bool> HandleAsync(User? user, object? resource, OperatorMode operatorMode, params PermissionType[] requirements)
        {
            if (user == null) return false;

            if (user.IsAdmin()) return true;

            if (user.HasValidPermissions(operatorMode, requirements)) return true;

            return false;
        }
    }
}


