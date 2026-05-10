using School_Management.Core.Attributes;
using School_Management.Core.Enums;

namespace School_Management.Core.Models
{
    [AuditIgnoreType(AuditOperation.All)]
    public class StudentPhoto
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? LocalPath { get; set; }

        public FileStatus FileStatus { get; set; }
        public DateTime? LastAttempt { get; set; }

        public Candidate Student { get; set; } = null!;
    }
}
