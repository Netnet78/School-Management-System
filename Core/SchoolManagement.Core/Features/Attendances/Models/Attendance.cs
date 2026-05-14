using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Features.AuditLogs.Enums;
using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Features.Employees.Models;
using SchoolManagement.Core.Features.Attendances.Enums;

namespace SchoolManagement.Core.Features.Attendances.Models
{
    [AuditIgnoreType(AuditOperation.Insert | AuditOperation.Update)]
    public class Attendance : IEntity
    {
        public int Id { get; set; }
        public TimeOnly ScanTime { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public int StudentClassId { get; set; }
        public StudentClass StudentClass { get; set; } = null!;
        public int? MarkedByEmployeeId { get; set; }
        public Employee? MarkedByEmployee { get; set; } = null!;
        public AttendanceStatus Status { get; set; }
        public string OtherInfo { get; set; } = string.Empty;
    }
}

