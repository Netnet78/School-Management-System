using SchoolManagement.Core.Features.Classes.Models;
using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Subjects.Models
{
    [Description("មុខវិជ្ជា")]
    public class Subject : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
        public decimal MaxScore { get; set; }
        public bool IsActive { get; set; } = true;
        public List<ClassSubject> ClassSubjects { get; set; } = [];
        public List<SubjectMapper> Mappers { get; set; } = [];
    }
}
