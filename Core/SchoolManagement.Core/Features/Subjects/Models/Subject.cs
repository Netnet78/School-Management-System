using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Subjects.Models
{
    public class Subject : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
        public decimal MaxScore { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<ClassSubject> ClassSubjects { get; set; } = [];
        public ICollection<SubjectComponent> Components { get; set; } = [];
    }
}
