namespace SchoolManagement.Infrastructure.Shared.Contracts;

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
        int? pageSize = null);
    async Task DeleteAsync(int id)
    {
        var data = await GetByIdAsync(id);
        if (data is null) return;
        await DeleteAsync(data);
    }

    public Task SaveAsync();
    public Task SaveAsync(MT entity);

    Task<IEnumerable<MT>> FindAsync(
        IEnumerable<FilterCondition<MT>>? filters = null,
        int? page = null,
        int? pageSize = null,
        IEnumerable<SortCriteria<MT>>? orderBy = null,
        params string[]? includes);
}
