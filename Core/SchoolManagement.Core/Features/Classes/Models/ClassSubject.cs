using SchoolManagement.Core.Features.Accessments.Models;
using SchoolManagement.Core.Features.Employees.Models;
using SchoolManagement.Core.Features.Subjects.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Classes.Models
{
    public class ClassSubject : IEntity
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public Class Class { get; set; } = null!;
        public int? TeacherId { get; set; }
        public Employee? Teacher { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public ICollection<Assessment> Scores { get; set; } = [];
    }
}
