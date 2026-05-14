using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace SchoolManagement.Infrastructure.Repositories;

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
        await SaveAsync();
    }

    public virtual async Task UpdateAsync(TEntity data)
    {
        ArgumentNullException.ThrowIfNull(data);

        int id = GetEntityId(data);
        TEntity? existing = await Set.FindAsync(id);

        if (existing is null)
        {
            Set.Attach(data);
            Context.Entry(data).State = EntityState.Modified;
        }
        else
        {
            Context.Entry(existing).CurrentValues.SetValues(data);
        }

        await SaveAsync();
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

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        IEnumerable<FilterCondition<TEntity>>? filters = null,
        int? page = null,
        int? pageSize = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params Expression<Func<TEntity, object?>>[]? includes)
    {
        IQueryable<TEntity> query = BuildQuery(filters, includes);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip(pageSize.Value * (page.Value - 1))
                .Take(pageSize.Value);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        int? page = null,
        int? pageSize = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params Expression<Func<TEntity, object?>>[]? includes)
    {
        IQueryable<TEntity> query = CreateQuery();

        if (includes != null)
        {
            foreach (Expression<Func<TEntity, object?>> include in includes)
            {
                query = query.Include(include);
            }
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

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
        int? pageSize = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params Expression<Func<TEntity, object?>>[]? includes)
    {
        IQueryable<TEntity> query = BuildQuery(filters, includes);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip(pageSize.Value * (page.Value - 1))
                .Take(pageSize.Value);
        }

        return await query.CountAsync();
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        int? page = null,
        int? pageSize = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params Expression<Func<TEntity, object?>>[]? includes)
    {
        IQueryable<TEntity> query = CreateQuery();

        if (includes != null)
        {
            foreach (Expression<Func<TEntity, object?>> include in includes)
            {
                query = query.Include(include);
            }
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

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
        params Expression<Func<TEntity, object?>>[]? includes)
    {
        IQueryable<TEntity> query = CreateQuery();

        if (includes != null)
        {
            foreach (Expression<Func<TEntity, object?>> include in includes)
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
