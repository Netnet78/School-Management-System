using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;
using System.Reflection;
using SchoolManagement.Infrastructure.Shared.Contracts;
using SchoolManagement.Infrastructure.Shared.Querying;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SchoolManagement.Infrastructure.Shared.Repositories;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : class, IEntity
{
    protected readonly SchoolDbContext Context;

    protected BaseRepository(SchoolDbContext context)
    {
        Context = context;
    }

    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    protected virtual IQueryable<TEntity> CreateQuery()
    {
        return Set;
    }

    public virtual IQueryable Query()
    {
        return CreateQuery();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await CreateQuery().ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await Set.FindAsync(id);
    }

    public virtual async Task AddAsync(TEntity data)
    {
        ArgumentNullException.ThrowIfNull(data);
        await Set.AddAsync(data);
        await SaveAsync(data);
    }

    public virtual async Task UpdateAsync(TEntity data)
    {
        ArgumentNullException.ThrowIfNull(data);

        int id = GetEntityId(data);

        EntityEntry<TEntity>? tracked = Context.ChangeTracker.Entries<TEntity>()
            .FirstOrDefault(e => e.Entity.Id == data.Id);

        if (tracked != null)
        {
            Context.Entry(tracked.Entity).CurrentValues.SetValues(data);
            await SaveAsync(tracked.Entity);
        }
        else
        {
            Context.Update(data);
            await SaveAsync(data);
        }
    }

    public virtual async Task DeleteAsync(TEntity data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Set.Remove(data);
        await SaveAsync();
    }

    public virtual async Task SaveAsync()
    {
        await Context.SaveChangesAsync();
    }

    public virtual async Task SaveAsync(TEntity entity)
    {
        await Context.SaveChangesAsync();
        await Context.Entry(entity).ReloadAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        IEnumerable<FilterCondition<TEntity>>? filters = null,
        int? page = null,
        int? pageSize = null,
        IEnumerable<SortCriteria<TEntity>>? orderBy = null,
        params string[]? includes)
    {
        IQueryable<TEntity> query = BuildQuery(filters, includes);

        query = query.ApplySorting(orderBy);

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip(pageSize.Value * (page.Value - 1))
                .Take(pageSize.Value);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<int> CountAsync(
        IEnumerable<FilterCondition<TEntity>>? filters = null,
        int? page = null,
        int? pageSize = null)
    {
        IQueryable<TEntity> query = BuildQuery(filters, null);

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip(pageSize.Value * (page.Value - 1))
                .Take(pageSize.Value);
        }

        return await query.CountAsync();
    }

    protected virtual IQueryable<TEntity> BuildQuery(
        IEnumerable<FilterCondition<TEntity>>? filters,
        params string[]? includes)
    {
        IQueryable<TEntity> query = CreateQuery();

        if (includes != null)
        {
            foreach (string include in includes)
            {
                query = query.Include(include);
            }
        }

        if (filters != null)
        {
            query = query.ApplyFilters(filters);
        }

        return query;
    }

    protected static int GetEntityId(TEntity entity)
    {
        PropertyInfo? property = typeof(TEntity).GetProperty("Id");
        if (property == null || property.PropertyType != typeof(int))
        {
            throw new InvalidOperationException($"{typeof(TEntity).Name} must expose an int Id property.");
        }

        object? value = property.GetValue(entity);
        if (value is null)
        {
            throw new InvalidOperationException($"{typeof(TEntity).Name} has a null Id value.");
        }

        return (int)value;
    }
}
