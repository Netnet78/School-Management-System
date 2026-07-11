using SchoolManagement.Core.Features.Attendances.Enums;

namespace SchoolManagement.Core.Features.Attendances.Models
{
    public class AttendanceDTO
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentGender { get; set; } = string.Empty;
        public int PresentCount { get; set; }
        public int ExcusedCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int HalfDayCount { get; set; }
        public Dictionary<AttendanceDay, AttendanceStatus> DailyAttendance { get; set; } = [];
    }
}
