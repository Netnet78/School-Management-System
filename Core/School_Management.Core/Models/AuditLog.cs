
using School_Management.Core.Attributes;
using School_Management.Core.Enums;

namespace School_Management.Core.Models
{
    [AuditIgnoreType(AuditOperation.All)]
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = ""; 
        public string? EntityName { get; set; }
        public string Description => string.IsNullOrWhiteSpace(CustomDescription) ? (User != null ?
            $"{EntityType} {EntityName} was {Action.ToLower()} by user {User.Username}"
            : $"{EntityType} {EntityName} was {Action.ToLower()} by unknown user")
            : CustomDescription;
        public string? CustomDescription { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}