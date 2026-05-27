using SchoolManagement.Core.Features.Attendances.Models;

namespace SchoolManagement.Application.Features.Attendances.Contracts
{
    public interface IAttendanceService : ICrudService<Attendance>
    {
        Task<ReturnResponse<int>> GetLateStudentCountToday();
        Task<ReturnResponse<int>> GetAbsentStudentCountToday();
        Task<ReturnResponse<int>> GetPresentStudentCountToday();
        Task<ReturnResponse<int>> GetExcusedStudentCountToday();
    }
}
