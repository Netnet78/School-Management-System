using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Features.Attendances.Models;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Features.Assessments.Models;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Students.Models
{
    [Description("ថ្នាក់រៀនសិស្ស")]
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

        public List<Assessment> Scores { get; set; } = [];
        public List<Attendance> Attendances { get; set; } = [];

        public string CustomAuditDescription()
        {
            return $"{Student.FullName} " +
                $"ត្រូវបានដាក់ចូលទៅក្នុង{Class.KhmerName}";
        }

        public string CustomAuditName()
        {
            return Student != null ? Student.FullName : string.Empty;
        }
    }
}
