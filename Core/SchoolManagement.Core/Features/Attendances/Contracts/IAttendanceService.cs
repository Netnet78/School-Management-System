using SchoolManagement.Core.Features.Attendances.Models;
using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Features.Attendances.Contracts
{
    public interface IAttendanceService : ICrudService<Attendance>
    {
        Task<ReturnResponse<int>> GetLateStudentCountToday();
        Task<ReturnResponse<int>> GetAbsentStudentCountToday();
        Task<ReturnResponse<int>> GetPresentStudentCountToday();
        Task<ReturnResponse<int>> GetExcusedStudentCountToday();
    }
}
