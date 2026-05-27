namespace SchoolManagement.Application.Features.Candidates.Contracts
{
    public interface ICandidateService
    {
        Task<ReturnResponse<IEnumerable<Candidate>>> GetAllAsync(int page, int pageSize,
            IEnumerable<FilterCondition<Candidate>>? filters,
            StudentDataStateFilterOptions dataState,
            string? sortBy,
            OrderDirection orderBy);
        Task<ReturnResponse<int>> GetAllCountAsync(
            IEnumerable<FilterCondition<Candidate>>? filters,
            StudentDataStateFilterOptions dataState);
        Task<ReturnResponse> DeleteCandidateAsync(int candidateId);
        Task<ReturnResponse> UpdateCandidateAsync(Candidate candidate);
    }
}
