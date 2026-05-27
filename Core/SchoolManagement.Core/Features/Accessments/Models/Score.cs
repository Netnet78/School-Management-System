using SchoolManagement.Core.Features.Subjects.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Accessments.Models
{
    public class Score : IEntity
    {
        public int Id { get; set; }
        public int AccessmentId { get; set; }
        public Assessment Assessment { get; set; } = null!;
        public int ComponentId { get; set; }
        public SubjectComponent Component { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}
