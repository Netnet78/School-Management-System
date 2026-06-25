using SchoolManagement.Core.Features.Assessments.Models;

namespace SchoolManagement.Application.Features.Grades.Authorization
{
    /// <summary>
    /// Handles authorization for Score-related operations.
    /// Admins have full access. Head Teachers and Teachers can manage scores for their students.
    /// </summary>
    public class ScoreAuthorizationHandler : IAuthorizationHandler
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

            if (resource is not Assessment score)
                return false;

            // Get the student's class
            Class? studentClass = score.StudentClass?.Class;
            if (studentClass == null) return false;

            // Head Teachers can manage scores in their department
            if (user.IsHeadTeacher())
            {
                return user.Employee?.DepartmentId == studentClass.Generation.DepartmentId;
            }

            // Teachers can manage scores for students in their classes
            bool isTeacherOfClass = user.Employee?.Classes?
                .Any(c => c.Id == studentClass.Id) == true;

            return isTeacherOfClass;
        }
    }
}


