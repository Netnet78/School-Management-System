using System.ComponentModel.DataAnnotations;

namespace School_Management.Core.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}