using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Models;

namespace School_Management.Application.Policies
{
    /// <summary>
    /// Handles authorization for Class-related operations.
    /// Admins have full access. Head Teachers can access classes in their department.
    /// Teachers can only view classes they teach.
    /// </summary>
    public class ClassAuthorizationHandler : IAuthorizationHandler
    {
        public async Task<bool> HandleAsync(User? user, object? resource, OperatorMode operatorMode, params PermissionType[] requirements)
        {
            if (user == null) return false;

            if (user.IsAdmin())
                return true;

            if (!user.HasValidPermissions(operatorMode, requirements))
                return false;

            if (resource == null)
                return true;

            if (resource is not Class @class)
                return false;

            // Head Teachers can manage classes in their department
            if (user.IsHeadTeacher())
            {
                return user.Employee?.DepartmentId == @class.Generation.DepartmentId;
            }

            // Teachers can only access classes they teach
            bool isTeacherOfClass = user.Employee?.Classes
                .Any(c => c.Id == @class.Id) == true;

            return isTeacherOfClass;
        }
    }
}
