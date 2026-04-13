using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IScoreRepository
    {
        Task<List<Score>> GetAllAsync();
        Task<Score?> GetByIdAsync(int id);
        Task AddAsync(Score score);
        Task UpdateAsync(Score score);
        Task DeleteAsync(Score score);
        Task SaveAsync();
    }
}