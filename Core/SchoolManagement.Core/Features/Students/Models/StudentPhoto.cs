using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Features.AuditLogs.Enums;
using SchoolManagement.Core.Features.Files.Enums;
using SchoolManagement.Core.Features.Candidates.Models;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Students.Models
{
    [Description("រូបភាពសិស្ស")]
    [AuditIgnoreType(AuditOperation.All)]
    public class StudentPhoto : IEntity
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? LocalPath { get; set; }

        public FileStatus FileStatus { get; set; }
        public DateTime? LastAttempt { get; set; }

        public Candidate Student { get; set; } = null!;
    }
}

