using System.Linq.Expressions;

namespace SchoolManagement.Application.Features.AuditLogs.Contracts
{
    public interface IAuditLogService : ICrudService<AuditLog>
    {
        Task<ReturnResponse<IEnumerable<AuditLog>>> GetAllAsync(
            int page,
            int? pageSize,
            IEnumerable<FilterCondition<AuditLog>>? filters,
            Expression<Func<AuditLog, bool>>? extraPredicate,
            IEnumerable<SortCriteria<AuditLog>>? orderBy,
            params string[]? includes);

        Task<ReturnResponse<int>> GetAllCountAsync(
            int page,
            int? pageSize,
            IEnumerable<FilterCondition<AuditLog>>? filters,
            Expression<Func<AuditLog, bool>>? extraPredicate);
    }
}
