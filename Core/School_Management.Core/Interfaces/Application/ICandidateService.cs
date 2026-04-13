using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Application
{
    public interface ICandidateService
    {
        Task<ReturnResponse<List<Candidate>>> GetAllCandidatesAsync(int? lastId, int pageSize);
        Task<ReturnResponse<int>> GetAllCandidatesCountAsync();
        Task<ReturnResponse> InsertCandidateAsync(Candidate candidate);
        Task<ReturnResponse> DeleteCandidateAsync(int candidateId);
        Task<ReturnResponse> EditCandidateAsync(Candidate candidate);
    }
}
