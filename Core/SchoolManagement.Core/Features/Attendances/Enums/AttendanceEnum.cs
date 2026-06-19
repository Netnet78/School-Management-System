using System.ComponentModel;

namespace SchoolManagement.Core.Features.Attendances.Enums
{
    public enum AttendanceStatus
    {
        [Description("វត្តមាន")]
        Present,
        [Description("អវត្តមាន")]
        Absent,
        [Description("យឺត")]
        Late,
        [Description("ច្បាប់")]
        Excused,
        [Description("ច្បាប់យឺត")]
        ExcusedLate,
        [Description("ច្បាប់ចេញទៅក្រៅ")]
        EarlyLeave
    }
}
