namespace SchoolManagement.Application.Features.Classes.Helpers
{
    public static class ClassFilters
    {
        public static IEnumerable<FilterCondition<Class>> BuildAccessFilter(User user)
        {
            if (string.Equals(user.Role?.Name, RoleType.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return [];
            }

            if (string.Equals(user.Role?.Name, RoleType.HeadTeacher.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                int departmentId = user.Employee?.DepartmentId ?? -1;
                return [
                    new(c => c.Generation.DepartmentId, FilterOperator.Equals, departmentId)
                ];
            }

            int teacherId = user.Employee?.Id ?? -1;
            return [
                new(c => c.TeacherId, FilterOperator.Equals, teacherId)
            ];
        }
    }
}
