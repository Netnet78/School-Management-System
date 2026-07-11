using SchoolManagement.Core.Features.Attendances.Models;

namespace SchoolManagement.Application.Features.Attendances.Contracts
{
    public interface IAttendanceService : ICrudService<Attendance>
    {
        Task<ReturnResponse<int>> GetLateStudentCountToday();
        Task<ReturnResponse<int>> GetAbsentStudentCountToday();
        Task<ReturnResponse<int>> GetPresentStudentCountToday();
        Task<ReturnResponse<int>> GetExcusedStudentCountToday();
        Task<ReturnResponse<IEnumerable<AttendanceDTO>>> GetClassMonthlyAttendance(int classId, int month, int year);
        Task<ReturnResponse<IEnumerable<Attendance>>> GetAttendancesWithCursorAsync(IEnumerable<FilterCondition<Attendance>>? filters, DateTime? lastDate, int? lastId, int pageSize, params string[] includes);
    }
}
