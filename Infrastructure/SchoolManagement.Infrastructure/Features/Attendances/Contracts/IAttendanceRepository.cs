using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Attendances.Contracts
{
    public interface IAttendanceRepository : IBaseRepository<Attendance>
    {
        Task<Attendance?> GetByStudentClassId(int studentClassId);
        Task<IEnumerable<Attendance>> GetAllFromStudentId(int studentId);
        Task<IEnumerable<Attendance>> GetAllFromStudentClassId(int studentClassId);
    }
}
