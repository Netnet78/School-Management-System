using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Policies
{
    /// <summary>
    /// Handles authorization for Candidate-related operations.
    /// Only Admins can manage candidates.
    /// </summary>
    public class CandidateAuthorizationHandler : IAuthorizationHandler
    {
        public async Task<bool> HandleAsync(User? user, object? resource, OperatorMode operatorMode, params PermissionType[] requirements)
        {
            if (user == null) return false;

            if (user.IsAdmin())
                return true;

            if (!user.HasValidPermissions(operatorMode, requirements))
                return false;

            // Candidates are admin-only resources
            return resource == null;
        }
    }
}
