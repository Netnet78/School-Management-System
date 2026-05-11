using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Models;
using System.Linq.Expressions;

namespace SchoolManagement.Core.Helpers
{
    public static class ClassFilters
    {
        public static Expression<Func<Class, bool>> BuildAccessFilter(User user)
        {
            if (string.Equals(user.Role?.Name, RoleType.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return @class => true;
            }

            if (string.Equals(user.Role?.Name, RoleType.HeadTeacher.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                int departmentId = user.Employee?.DepartmentId ?? -1;
                return @class => @class.Generation.DepartmentId == departmentId;
            }

            int teacherId = user.Employee?.Id ?? -1;
            return @class => @class.TeacherId == teacherId;
        }
    }
}
