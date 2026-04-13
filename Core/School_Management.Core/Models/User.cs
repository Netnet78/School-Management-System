using System.ComponentModel.DataAnnotations;

namespace School_Management.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastLogin { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockedOutEnd { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public ICollection<AuditLog> AuditLogs { get; set; } = [];
        public ICollection<Employee> Employees { get; set; } = [];
    }
}