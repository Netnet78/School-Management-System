using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface ICandidateRepository
    {
        Task<List<Candidate>> GetAllAsync(int? lastId, int pageSize);
        Task<List<Candidate>> GetCandidatesOnlyAsync(int? lastId, int pageSize);
        Task<int> GetCandidatesOnlyCountAsync();
        Task<int> GetAllCountAsync();
        Task<Candidate?> GetByIdAsync(int id);
        Task AddAsync(Candidate candidate);
        Task UpdateAsync(Candidate candidate);
        Task DeleteAsync(Candidate candidate);
        Task SaveAsync();
    }
}