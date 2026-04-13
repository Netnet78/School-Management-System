using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IStudentClassRepository
    {
        Task<List<StudentClass>> GetAllAsync();
        Task<StudentClass?> GetByIdAsync(int id);
        Task<List<StudentClass>?> GetAllFromStudentIdAsync(int studentId);
        Task AddAsync(StudentClass studentClass);
        Task UpdateAsync(StudentClass studentClass);
        Task DeleteAsync(StudentClass studentClass);
        Task SaveAsync();
    }
}