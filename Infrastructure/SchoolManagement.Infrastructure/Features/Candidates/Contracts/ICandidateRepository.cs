using SchoolManagement.Core.Features.Candidates.DTOs;
using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Candidates.Contracts
{
    public interface ICandidateRepository : IBaseRepository<Candidate>
    {
        Task<IEnumerable<Candidate>> GetPagedAsync(int page, int pageSize,
            IEnumerable<FilterCondition<Candidate>>? filters,
            StudentDataStateFilterOptions dataState,
            string? sortBy,
            OrderDirection orderBy);
        Task<int> GetCountAsync(
            IEnumerable<FilterCondition<Candidate>>? filters,
            StudentDataStateFilterOptions dataState);
        Task<CandidateDashboardMetrics> GetDashboardMetricsAsync(int? daysFilter, DateTime todayUtc);
    }
}
