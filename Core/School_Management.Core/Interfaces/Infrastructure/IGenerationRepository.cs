using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IGenerationRepository
    {
        Task<List<Generation>> GetAllAsync();
        Task<Generation?> GetByIdAsync(int id);
        Task AddAsync(Generation generation);
        Task UpdateAsync(Generation generation);
        Task DeleteAsync(Generation generation);
        Task SaveAsync();
    }
}