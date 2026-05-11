using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Policies
{
    /// <summary>
    /// Handles authorization for Employee-related operations.
    /// Only Admins can manage employees.
    /// Head Teachers can view employees in their department.
    /// </summary>
    public class EmployeeAuthorizationHandler : IAuthorizationHandler
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

            if (resource is not Employee employee)
                return false;

            // Head Teachers can view employees in their department
            if (user.IsHeadTeacher())
            {
                return user.Employee?.DepartmentId == employee.DepartmentId;
            }

            // Regular teachers cannot access employee resources with specific resource
            return false;
        }
    }
}
