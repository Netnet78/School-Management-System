namespace SchoolManagement.Application.Features.Classes.Authorization
{
    public static class AuthorizationClassHelper
    {
        public static bool IsAdmin(this User user)
        {
            return user.Role?.Name.Equals(RoleType.Admin.ToString(), StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsHeadTeacher(this User user)
        {
            return user.Role?.Name.Equals(RoleType.HeadTeacher.ToString(), StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsValidRole(this User user, RoleType role)
        {
            return user.Role?.Name.Equals(role.ToString(), StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool HasValidPermissions(this User user, OperatorMode operatorMode, params PermissionType[] requirements)
        {
            HashSet<string> userPermissions = user.Role?.Permissions?
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? [];

            bool hasAny = false;

            foreach (PermissionType requirement in requirements)
            {
                if (operatorMode == OperatorMode.AND)
                {
                    if (!userPermissions.Contains(requirement.ToString()))
                        return false;
                }
                else
                {
                    if (userPermissions.Contains(requirement.ToString()))
                    {
                        hasAny = true;
                        break;
                    }
                }
            }

            if (!hasAny && operatorMode == OperatorMode.OR) return false;

            return true;
        }
    }
}

