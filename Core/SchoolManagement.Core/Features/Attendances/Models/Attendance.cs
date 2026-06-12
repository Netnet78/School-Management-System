using SchoolManagement.Core.Shared.Attributes;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Features.AuditLogs.Enums;
using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Features.Employees.Models;
using SchoolManagement.Core.Features.Attendances.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Core.Features.Attendances.Models
{
    [AuditIgnoreType(AuditOperation.Insert | AuditOperation.Update)]
    public class Attendance : IEntity
    {
        public int Id { get; set; }
        public DateTime AttendanceDateTime { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public DateOnly AttendanceDate
        {
            get => DateOnly.FromDateTime(AttendanceDateTime);
            set => AttendanceDateTime = value.ToDateTime(ScanTime);
        }

        [NotMapped]
        public TimeOnly ScanTime
        {
            get => TimeOnly.FromDateTime(AttendanceDateTime);
            set => AttendanceDateTime = AttendanceDate.ToDateTime(value);
        }

        public DateTime LocalAttendanceDateTime => AttendanceDateTime.ToLocalTime();
        public DateOnly LocalAttendanceDate => DateOnly.FromDateTime(LocalAttendanceDateTime);
        public TimeOnly LocalScanTime => TimeOnly.FromDateTime(LocalAttendanceDateTime);

        public int StudentClassId { get; set; }
        public StudentClass StudentClass { get; set; } = null!;
        public int? MarkedByEmployeeId { get; set; }
        public Employee? MarkedByEmployee { get; set; } = null!;
        public AttendanceStatus Status { get; set; }
        public string OtherInfo { get; set; } = string.Empty;
    }
}

