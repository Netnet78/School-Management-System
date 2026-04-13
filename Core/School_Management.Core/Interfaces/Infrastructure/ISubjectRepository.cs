using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface ISubjectRepository
    {
        Task<List<Subject>> GetAllAsync();
        Task<Subject?> GetByIdAsync(int id);
        Task AddAsync(Subject subject);
        Task UpdateAsync(Subject subject);
        Task DeleteAsync(Subject subject);
        Task SaveAsync();
    }
}