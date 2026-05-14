namespace SchoolManagement.Application.Features.Classes.Authorization
{
    public static class AuthorizationClassHelper
    {
        public static bool IsAdmin(this User user)
        {
            if (user.Role.Name.Equals(RoleType.Admin.ToString(), StringComparison.CurrentCultureIgnoreCase)) return true;
            else return false;
        }

        public static bool IsHeadTeacher(this User user)
        {
            if (user.Role.Name.Equals(RoleType.HeadTeacher.ToString(), StringComparison.CurrentCultureIgnoreCase)) return true;
            else return false;
        }

        public static bool IsValidRole(this User user, RoleType role)
        {
            if (user.Role.Name.Equals(role.ToString(), StringComparison.CurrentCultureIgnoreCase)) return true;
            else return false;
        }

        public static bool HasValidPermissions(this User user, OperatorMode operatorMode, PermissionType[] requirements)
        {
            HashSet<string> userPermissions = user.Role.Permissions
                .Select(p => p.Name)
                .ToHashSet();

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

            if (!hasAny && operatorMode.Equals(OperatorMode.OR)) return false;

            return true;
        }
    }
}

