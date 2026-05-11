using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Policies
{
    /// <summary>
    /// Handles authorization for Grade-related operations.
    /// Admins have full access. Teachers and Head Teachers can view grades.
    /// Grade is primarily an admin/system resource.
    /// </summary>
    public class GradeAuthorizationHandler : IAuthorizationHandler
    {
        public async Task<bool> HandleAsync(User? user, object? resource, OperatorMode operatorMode, params PermissionType[] requirements)
        {
            if (user == null) return false;

            if (user.IsAdmin())
                return true;

            if (!user.HasValidPermissions(operatorMode, requirements))
                return false;

            // Grade is primarily an admin/system resource
            // Teachers can view but not modify specific grades
            // Resource-level checks are minimal since Grade doesn't have department hierarchy
            return resource == null;
        }
    }
}
