using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IAttendanceRepository : IBaseRepository<Attendance>
    {
        Task<Attendance?> GetByStudentClassId(int studentClassId);
        Task<IEnumerable<Attendance>> GetAllFromStudentId(int studentId);
        Task<IEnumerable<Attendance>> GetAllFromStudentClassId(int studentClassId);
    }
}
