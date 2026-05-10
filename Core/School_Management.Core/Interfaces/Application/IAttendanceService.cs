using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Application
{
    public interface IAttendanceService : ICrudService<Attendance>
    {
        Task<ReturnResponse<int>> GetLateStudentCountToday();
        Task<ReturnResponse<int>> GetAbsentStudentCountToday();
        Task<ReturnResponse<int>> GetPresentStudentCountToday();
        Task<ReturnResponse<int>> GetExcusedStudentCountToday();
    }
}
