using SchoolManagement.Core.Features.Departments.Models;
using SchoolManagement.Core.Features.Employees.Models;
using SchoolManagement.Core.Features.Generations.Models;
using SchoolManagement.Core.Features.Grades.Models;
using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Classes.Models
{
    public class Class : IEntity
    {
        public int Id { get; set; }
        public int GradeId { get; set; }
        public Grade Grade { get; set; } = null!;
        public int GenerationId { get; set; }
        public Generation Generation { get; set; } = null!;
        public Department Department => Generation.Department;
        public int? TeacherId { get; set; }
        public Employee? Teacher { get; set; } = null;
        /// <summary>
        /// Name of the class with the following format:
        /// <see cref="Name"/> <see cref="Department.Name"/> <see cref="Generation.CohortNumber"/>
        /// "[Class name] [Department name] [Generation cohort number]"
        /// </summary>
        public string Name => Grade != null ? 
            $"{Grade.Name} {Generation.Department.Name} ជំនាន់ទី {Generation.CohortNumber}" 
            : string.Empty;

        public ICollection<StudentClass> Students { get; set; } = [];
        public ICollection<ClassSubject> Subjects { get; set; } = [];
    }
}
