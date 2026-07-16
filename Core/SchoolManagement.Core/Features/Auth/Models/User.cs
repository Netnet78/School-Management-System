using SchoolManagement.Core.Features.AuditLogs.Models;
using SchoolManagement.Core.Features.Employees.Models;
using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Core.Shared.Contracts;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Core.Features.Auth.Models
{
    [Description("អ្នកប្រើប្រាស់")]
    public class User : IEntity
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [AuditHide(ShowMasked = true)]
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [AuditIgnore]
        public DateTime? LastLogin { get; set; }
        [AuditIgnore]
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockedOutEnd { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public List<AuditLog> AuditLogs { get; set; } = [];
    }
}
