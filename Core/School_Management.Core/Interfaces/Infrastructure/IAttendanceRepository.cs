using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IAttendanceRepository
    {
        Task<List<Attendance>> GetAllAsync();
        Task<Attendance?> GetByIdAsync(int id);
        Task<Attendance?> GetByStudentClassId(int studentClassId);
        Task<List<Attendance>> GetAllFromStudentId(int studentId);
        Task<List<Attendance>> GetAllFromStudentClassId(int studentClassId);
        Task AddAsync(Attendance attendance);
        Task UpdateAsync(Attendance attendance);
        Task DeleteAsync(Attendance attendance);
        Task SaveAsync();
    }
}