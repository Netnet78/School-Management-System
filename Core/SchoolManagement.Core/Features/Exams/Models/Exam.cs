using SchoolManagement.Core.Features.Accessments.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Exams.Models
{
    public class Exam : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KhmerName { get; set; } = string.Empty;
        public ICollection<Assessment> Scores { get; set; } = [];
    }
}
