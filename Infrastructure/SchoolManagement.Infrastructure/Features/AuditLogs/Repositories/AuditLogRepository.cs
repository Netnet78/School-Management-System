using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Querying;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.AuditLogs.Repositories;

public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AuditLog>> FindAsync(
        IEnumerable<FilterCondition<AuditLog>>? filters,
        Expression<Func<AuditLog, bool>>? extraPredicate,
        int? page,
        int? pageSize,
        IEnumerable<SortCriteria<AuditLog>>? orderBy,
        params string[]? includes)
    {
        IQueryable<AuditLog> query = BuildQuery(filters, includes);

        if (extraPredicate != null)
        {
            query = query.Where(extraPredicate);
        }

        query = query.ApplySorting(orderBy);

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip(pageSize.Value * (page.Value - 1))
                .Take(pageSize.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<int> CountAsync(
        IEnumerable<FilterCondition<AuditLog>>? filters,
        Expression<Func<AuditLog, bool>>? extraPredicate,
        int? page,
        int? pageSize)
    {
        IQueryable<AuditLog> query = BuildQuery(filters, null);

        if (extraPredicate != null)
        {
            query = query.Where(extraPredicate);
        }

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip(pageSize.Value * (page.Value - 1))
                .Take(pageSize.Value);
        }

        return await query.CountAsync();
    }
}
