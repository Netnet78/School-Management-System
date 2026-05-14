using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Features.AuditLogs.Enums;
using SchoolManagement.Core.Features.Files.Enums;

namespace SchoolManagement.Core.Features.Employees.Models
{
    [AuditIgnoreType(AuditOperation.All)]
    public class EmployeePhoto : IEntity
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? LocalPath { get; set; }

        public FileStatus FileStatus { get; set; }
        public DateTime? LastAttempt { get; set; }

        public Employee? Employee { get; set; }
    }
}

