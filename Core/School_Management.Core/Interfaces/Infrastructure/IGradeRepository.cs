using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IGradeRepository
    {
        Task<List<Grade>> GetAllAsync();
        Task<Grade?> GetByIdAsync(int id);
        Task AddAsync(Grade grade);
        Task UpdateAsync(Grade grade);
        Task DeleteAsync(Grade grade);
        Task SaveAsync();
    }
}