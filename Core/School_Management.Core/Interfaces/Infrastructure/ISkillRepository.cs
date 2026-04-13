using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface ISkillRepository
    {
        Task AddAsync(Skill skill);
        Task DeleteAsync(Skill skill);
        Task<List<Skill>> GetAllAsync();
        Task<Skill?> GetByIdAsync(int id);
        Task SaveAsync();
        Task UpdateAsync(Skill skill);
    }
}
