using School_Management.Core.Attributes;
using School_Management.Core.Enums;

namespace School_Management.Core.Models
{
    [AuditIgnoreType(AuditOperation.All)]
    public class StudentQR
    {
        public int Id { get; set; }
        public string QRCodeValue { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Student Student { get; set; } = null!;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
