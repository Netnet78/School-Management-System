using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface ICandidateRepository : IBaseRepository<Candidate>
    {
        Task<IEnumerable<Candidate>> GetAllAsync(int? lastId, int pageSize);
        Task<IEnumerable<Candidate>> GetAllPagedAsync(int page, int pageSize);
        Task<IEnumerable<Candidate>> GetAllPagedAsync(int page, int pageSize, StudentFilterOptions options);
        Task<IEnumerable<Candidate>> GetCandidatesOnlyAsync(int? lastId, int pageSize);
        Task<IEnumerable<Candidate>> GetCandidatesOnlyPagedAsync(int page, int pageSize);
        Task<IEnumerable<Candidate>> GetCandidatesOnlyPagedAsync(int page, int pageSize, StudentFilterOptions options);
        Task<int> GetCandidatesOnlyCountAsync();
        Task<int> GetCandidatesOnlyCountAsync(StudentFilterOptions options);
        Task<int> GetAllCountAsync();
        Task<int> GetAllCountAsync(StudentFilterOptions options);
    }
}
