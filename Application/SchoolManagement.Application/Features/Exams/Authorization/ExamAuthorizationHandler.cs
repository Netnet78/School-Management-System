using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Policies
{
    /// <summary>
    /// Handles authorization for Exam-related operations.
    /// Admins have full access. Teachers can view exams but typically admins manage them.
    /// Exam is primarily an admin/system resource.
    /// </summary>
    public class ExamAuthorizationHandler : IAuthorizationHandler
    {
        public async Task<bool> HandleAsync(User? user, object? resource, OperatorMode operatorMode, params PermissionType[] requirements)
        {
            if (user == null) return false;

            if (user.IsAdmin())
                return true;

            if (!user.HasValidPermissions(operatorMode, requirements))
                return false;

            // Exam is primarily an admin/system resource
            // Teachers can view but not modify specific exams
            // Resource-level checks are minimal since Exam doesn't have direct department hierarchy
            return resource == null;
        }
    }
}
