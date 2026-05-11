using SchoolManagement.Core.Helpers;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Tests
{
    public class ClassFiltersTest
    {
        [Fact]
        public void BuildAccessFilter_Admin_CanSeeAllClasses()
        {
            User admin = new()
            {
                Role = new Role { Name = "Admin" }
            };

            Func<Class, bool> filter = ClassFilters.BuildAccessFilter(admin).Compile();

            Assert.True(filter(new Class { Id = 1, TeacherId = 10, Generation = new Generation { DepartmentId = 2 } }));
            Assert.True(filter(new Class { Id = 2, TeacherId = 11, Generation = new Generation { DepartmentId = 3 } }));
        }

        [Fact]
        public void BuildAccessFilter_HeadTeacher_OnlySeesClassesInOwnDepartment()
        {
            User headTeacher = new()
            {
                Role = new Role { Name = "HeadTeacher" },
                Employee = new Employee { DepartmentId = 7 }
            };

            Func<Class, bool> filter = ClassFilters.BuildAccessFilter(headTeacher).Compile();

            Assert.True(filter(new Class { Generation = new Generation { DepartmentId = 7 } }));
            Assert.False(filter(new Class { Generation = new Generation { DepartmentId = 8 } }));
        }

        [Fact]
        public void BuildAccessFilter_Teacher_OnlySeesOwnClasses()
        {
            User teacher = new()
            {
                Role = new Role { Name = "Teacher" },
                Employee = new Employee { Id = 42 }
            };

            Func<Class, bool> filter = ClassFilters.BuildAccessFilter(teacher).Compile();

            Assert.True(filter(new Class { TeacherId = 42 }));
            Assert.False(filter(new Class { TeacherId = 99 }));
        }
    }
}
