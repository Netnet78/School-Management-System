using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Exams.Models
{
    [Description("ប្រឡង")]
    public class Exam : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
        public ICollection<Assessment> Scores { get; set; } = [];
    }
}
