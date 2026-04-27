using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Application
{
    public interface ICandidateService
    {
        Task<ReturnResponse<IEnumerable<Candidate>>> GetAllAsync(int page, int pageSize);
        Task<ReturnResponse<IEnumerable<Candidate>>> GetAllAsync(int page, int pageSize, StudentFilterOptions options);
        Task<ReturnResponse<int>> GetAllCountAsync(StudentFilterOptions options);
        Task<ReturnResponse> InsertCandidateAsync(Candidate candidate);
        Task<ReturnResponse> DeleteCandidateAsync(int candidateId);
        Task<ReturnResponse> EditCandidateAsync(Candidate candidate);
    }
}
