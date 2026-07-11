using SchoolManagement.Core.Shared.Enums;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Features.Employees.Enums;
using SchoolManagement.Core.Features.Auth.Models;
using SchoolManagement.Core.Features.Departments.Models;
using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Features.Attendances.Models;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Employees.Models
{
    [Description("បុគ្គលិក")]
    public class Employee : IEntity, IAuditableEntity
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string LatinFullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        [Description("ស្ថានភាព")]
        public string IsActiveReadable => IsActive ? "នៅបម្រើ" : "ឈប់បម្រើ";
        public DateOnly HiredDate { get; set; }
        public Gender Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
        [Description("អាយុ")]
        public int Age
        {
            get
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                int age = today.Year - DateOfBirth.Year;

                if (today < DateOfBirth.AddYears(age))
                {
                    age--;
                }

                return age;
            }
        }
        public string PlaceOfBirth { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public MaritalStatus MaritalStatus { get; set; }
        public string PhotoKey => Photo?.Key ?? string.Empty;

        public int? DepartmentId { get; set; }
        public User? User { get; set; }
        public Department? Department { get; set; }
        public EmployeePhoto? Photo { get; set; }

        public List<Class> Classes { get; set; } = [];
        public List<ClassSubject> ClassSubjects { get; set; } = [];
        public List<Attendance> MarkedAttendances { get; set; } = [];

        // Salary section
        public DateOnly SalaryDate { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deduction { get; set; }
        public decimal Tax { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CustomAuditDescription()
        {
            return "";
        }

        public string CustomAuditName()
        {
            return $"{FullName}";
        }
    }
}
