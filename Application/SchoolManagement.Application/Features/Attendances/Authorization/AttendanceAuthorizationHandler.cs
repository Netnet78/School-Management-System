using AttendanceModel = SchoolManagement.Core.Features.Attendances.Models.Attendance;

namespace SchoolManagement.Application.Features.Attendances.Authorization
{
    public class AttendanceAuthorizationHandler : IAuthorizationHandler
    {
        public async Task<bool> HandleAsync(User? user, object? resource, OperatorMode operatorMode, params PermissionType[] requirements)
        {
            if (user == null) return false;

            if (user.IsAdmin())
                return true;

            bool isNullable = resource == null;

            if (!isNullable)
            {
                if (resource is not AttendanceModel attendance) return false;

                int? attendanceDepartmentId = attendance.StudentClass?.Class?.Generation?.DepartmentId;
                if (attendanceDepartmentId == null)
                    return false;

                if (user.Employee?.DepartmentId != attendanceDepartmentId)
                    return false;
            }

            if (!user.HasValidPermissions(operatorMode, requirements)) return false;

            return true;
        }
    }
}


