using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Features.AuditLogs.Enums;
using System.ComponentModel;

namespace SchoolManagement.Core.Features.Students.Models
{
    [Description("កូដ QR សិស្ស")]
    [AuditIgnoreType(AuditOperation.All)]
    public class StudentQR : IEntity
    {
        public int Id { get; set; }
        public string QRCodeValue { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Student Student { get; set; } = null!;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public StudentQR()
        {

        }

        public StudentQR(Student student)
        {
            QRCodeValue = Guid.NewGuid().ToString();
            Student = student;
        }
    }
}

