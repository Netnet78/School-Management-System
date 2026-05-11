using SchoolManagement.Core.Models;

namespace SchoolManagement.Core.Infrastructure.Interfaces
{
    public interface IAttendanceRepository : IBaseRepository<Attendance>
    {
        Task<Attendance?> GetByStudentClassId(int studentClassId);
        Task<IEnumerable<Attendance>> GetAllFromStudentId(int studentId);
        Task<IEnumerable<Attendance>> GetAllFromStudentClassId(int studentClassId);
    }
}
