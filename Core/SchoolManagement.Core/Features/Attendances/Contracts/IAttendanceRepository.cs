using SchoolManagement.Core.Features.Attendances.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.Attendances.Contracts
{
    public interface IAttendanceRepository : IBaseRepository<Attendance>
    {
        Task<Attendance?> GetByStudentClassId(int studentClassId);
        Task<IEnumerable<Attendance>> GetAllFromStudentId(int studentId);
        Task<IEnumerable<Attendance>> GetAllFromStudentClassId(int studentClassId);
    }
}
