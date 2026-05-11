using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Core.Enums;

namespace SchoolManagement.Core.Models
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

