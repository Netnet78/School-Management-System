using Microsoft.EntityFrameworkCore;
using School_Management.Core.Enums;

namespace School_Management.Core.Models
{
    [Index(nameof(UserId), IsUnique = true)]
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateOnly HiredDate { get; set; }
        public Gender Gender { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public MaritalStatus MaritalStatus { get; set; }
        public string PhotoPath { get; set; } = string.Empty;

        public int? UserId { get; set; }
        public int? DepartmentId { get; set; }
        public User? User { get; set; }
        public Department? Department { get; set; }

        public ICollection<Class> Classes { get; set; } = [];
        public ICollection<ClassSubject> ClassSubjects { get; set; } = [];
        public ICollection<Attendance> MarkedAttendances { get; set; } = [];

        // Salary section
        public DateOnly SalaryDate { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deduction { get; set; }
        public decimal Tax { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}