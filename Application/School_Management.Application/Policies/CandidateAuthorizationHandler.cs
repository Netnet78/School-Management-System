using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Models;

namespace School_Management.Application.Policies
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
