using SchoolManagement.Core.Shared.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [AuditMask]
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockedOutEnd { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; } = [];
    }
}
