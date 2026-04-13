using School_Management.Core.Enums;

namespace School_Management.Core.Models
{
    public class Attendance
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
