using SchoolManagement.Core.Features.Accessments.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Subjects.Models
{
    public class SubjectComponent : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;
        public ICollection<Score> Scores { get; set; } = [];
    }
}
