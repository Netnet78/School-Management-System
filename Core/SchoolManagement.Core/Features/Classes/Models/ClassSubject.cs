using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Core.Features.Employees.Models;
using SchoolManagement.Core.Features.Subjects.Models;
using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Classes.Models
{
    [Description("មុខវិជ្ជាថ្នាក់រៀន")]
    public class ClassSubject : IEntity, IAuditableEntity
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public Class Class { get; set; } = null!;
        public int? TeacherId { get; set; }
        public Employee? Teacher { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public List<Assessment> Scores { get; set; } = [];

        public string CustomAuditDescription()
        {
            return $"{Subject.KhmerName} បង្រៀនដោយ {Teacher?.FullName} " +
                $"ត្រូវបានបន្ថែមចូលទៅក្នុង {Class.KhmerName}";
        }

        public string CustomAuditName()
        {
            return $"";
        }
    }
}
