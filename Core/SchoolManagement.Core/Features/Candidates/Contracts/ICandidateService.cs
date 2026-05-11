using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Application.Interfaces
{
    public interface ICandidateService
    {
        Task<ReturnResponse<IEnumerable<Candidate>>> GetAllAsync(int page, int pageSize);
        Task<ReturnResponse<IEnumerable<Candidate>>> GetAllAsync(int page, int pageSize, StudentFilterOptions options);
        Task<ReturnResponse<int>> GetAllCountAsync(StudentFilterOptions options);
        Task<ReturnResponse> InsertCandidateAsync(Candidate candidate);
        Task<ReturnResponse> DeleteCandidateAsync(int candidateId);
        Task<ReturnResponse> UpdateCandidateAsync(Candidate candidate);
    }
}
