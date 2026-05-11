using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Application.Interfaces
{
    public interface IAttendanceService : ICrudService<Attendance>
    {
        Task<ReturnResponse<int>> GetLateStudentCountToday();
        Task<ReturnResponse<int>> GetAbsentStudentCountToday();
        Task<ReturnResponse<int>> GetPresentStudentCountToday();
        Task<ReturnResponse<int>> GetExcusedStudentCountToday();
    }
}
