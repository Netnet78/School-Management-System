using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Models;

namespace School_Management.Application.Policies
{
    public class AttendanceAuthorizationHandler : IAuthorizationHandler
    {
        public async Task<bool> HandleAsync(User user, object? resource, OperatorMode operatorMode, params PermissionType[] requirements)
        {
            if (user == null) return false;

            if (user.IsAdmin())
                return true;

            bool isNullable = resource == null;

            if (!isNullable)
            {
                if (resource is not Attendance attendance) return false;

                if (user.Employee?.DepartmentId != 
                    attendance.StudentClass.Class.Generation.DepartmentId)
                    return false;
            }

            if (!user.HasValidPermissions(operatorMode, requirements)) return false;

            return true;
        }
    }
}
