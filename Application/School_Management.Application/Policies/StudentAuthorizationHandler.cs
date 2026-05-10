using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Models;

namespace School_Management.Application.Policies
{
    public class StudentAuthorizationHandler : IAuthorizationHandler
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

            if (resource is not Student student)
                return false;

            Class? studentClass = student.Classes
                .FirstOrDefault(sc => sc.IsActive)?
                .Class;

            if (studentClass == null) return false;

            if (user.IsHeadTeacher())
            {
                return user.Employee?.DepartmentId ==
                    studentClass.Generation.DepartmentId;
            }

            bool isTeacherOfClass = user.Employee?.Classes
                .Any(c => c.Id == studentClass.Id) == true;

            return isTeacherOfClass;
            
        }
    }
}
