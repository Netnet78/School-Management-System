using System.Linq.Expressions;
using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.AuditLogs.Contracts
{
    public interface IAuditLogRepository : IBaseRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> FindAsync(
            IEnumerable<FilterCondition<AuditLog>>? filters,
            Expression<Func<AuditLog, bool>>? extraPredicate,
            int? page,
            int? pageSize,
            IEnumerable<SortCriteria<AuditLog>>? orderBy,
            params string[]? includes);

        Task<int> CountAsync(
            IEnumerable<FilterCondition<AuditLog>>? filters,
            Expression<Func<AuditLog, bool>>? extraPredicate,
            int? page,
            int? pageSize);
    }
}
