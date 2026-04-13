using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IClassRepository
    {
        Task<List<Class>> GetAllAsync();
        Task<Class?> GetByIdAsync(int id);
        Task AddAsync(Class @class);
        Task UpdateAsync(Class @class);
        Task DeleteAsync(Class @class);
        Task SaveAsync();
    }
}