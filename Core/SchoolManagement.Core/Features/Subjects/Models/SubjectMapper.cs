using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Subjects.Models
{
    [AuditIgnoreType(AuditLogs.Enums.AuditOperation.All)]
    public class SubjectMapper : IEntity
    {
        public int Id { get; set; }
        public int ComponentId { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;
        public SubjectComponent Component { get; set; } = null!;
        public List<Score> Scores { get; set; } = [];
    }
}
