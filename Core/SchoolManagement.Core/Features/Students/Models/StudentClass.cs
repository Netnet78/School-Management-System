using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Features.Attendances.Models;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Features.Accessments.Models;

namespace SchoolManagement.Core.Features.Students.Models
{
    public class StudentClass : IEntity, IAuditableEntity
    {
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public int ClassId { get; set; }
        public Class Class { get; set; } = null!;
        public bool IsActive { get; set; } = false;

        public ICollection<Assessment> Scores { get; set; } = [];
        public ICollection<Attendance> Attendances { get; set; } = [];

        public string GetAuditName()
        {
            return Student != null ? Student.FullName : string.Empty;
        }
    }
}
