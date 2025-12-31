using Student_Management.Models;
using System.Reflection;

namespace Student_Management.Services
{
    public static class StudentValidationService
    {
        public static bool HasAllData(this Student s)
        {
            var properties = typeof(Student).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(s);

                // Skip some properties
                if (prop.Name == nameof(Student.Id)
                    || prop.Name == nameof(Student.Age)
                    || prop.Name == nameof(Student.OtherInfo)
                    || prop.Name == nameof(Student.CreatedAt)
                    || prop.Name == nameof(Student.LatinFullName)
                    || prop.Name == nameof(Student.FullName))
                    continue;

                if (value == null) return false;

                if (value is string str && string.IsNullOrWhiteSpace(str))
                    return false;
            }

            return true;
        }
    }
}
