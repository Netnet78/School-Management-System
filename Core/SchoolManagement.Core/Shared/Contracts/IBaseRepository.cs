using SchoolManagement.Core.Shared.Models;
using System.Linq.Expressions;

namespace SchoolManagement.Core.Shared.Contracts;

public interface IBaseRepository<MT>
{
    Task<IEnumerable<MT>> GetAllAsync();
    Task<MT?> GetByIdAsync(int id);
    Task AddAsync(MT data);
    Task UpdateAsync(MT data);
    Task DeleteAsync(MT data);
    Task<int> CountAsync(
        IEnumerable<FilterCondition<MT>>? filters = null,
        int? page = null,
        int? pageSize = null,
        Func<IQueryable<MT>, IOrderedQueryable<MT>>? orderBy = null,
        params Expression<Func<MT, object?>>[]? includes);
    Task<int> CountAsync(
        Expression<Func<MT, bool>>? predicate = null,
        int? page = null,
        int? pageSize = null,
        Func<IQueryable<MT>, IOrderedQueryable<MT>>? orderBy = null,
        params Expression<Func<MT, object?>>[]? includes);
    async Task DeleteAsync(int id)
    {
        var data = await GetByIdAsync(id);
        if (data is null) return;
        await DeleteAsync(data);
    }

    public Task SaveAsync();

    Task<IEnumerable<MT>> FindAsync(
        IEnumerable<FilterCondition<MT>>? filters = null,
        int? page = null,
        int? pageSize = null,
        Func<IQueryable<MT>, IOrderedQueryable<MT>>? orderBy = null,
        params Expression<Func<MT, object?>>[]? includes);
    Task<IEnumerable<MT>> FindAsync(
        Expression<Func<MT, bool>>? predicate = null,
        int? page = null,
        int? pageSize = null,
        Func<IQueryable<MT>, IOrderedQueryable<MT>>? orderBy = null,
        params Expression<Func<MT, object?>>[]? includes);
}
