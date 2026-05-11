using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Core.Enums;

namespace SchoolManagement.Core.Models
{
    [AuditIgnoreType(AuditOperation.All)]
    public class EmployeePhoto
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? LocalPath { get; set; }

        public FileStatus FileStatus { get; set; }
        public DateTime? LastAttempt { get; set; }

        public Employee? Employee { get; set; }
    }
}

