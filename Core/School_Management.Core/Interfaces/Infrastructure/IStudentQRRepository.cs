using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IStudentQRRepository
    {
        Task<List<StudentQR>> GetAllAsync();
        Task<StudentQR?> GetByIdAsync(int id);
        Task<StudentQR?> GetByQRValueAsync(string value);
        Task AddAsync(StudentQR studentQR);
        Task UpdateAsync(StudentQR studentQR);
        Task DeleteAsync(StudentQR studentQR);
        Task SaveAsync();
    }
}