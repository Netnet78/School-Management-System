using School_Management.Core.Models;
using System.Linq;
using System.Linq.Expressions;

namespace School_Management.Core.Interfaces.Application
{
    public interface ICrudService<TEntity>
    {
        Task<ReturnResponse<IEnumerable<TEntity>>> GetAllAsync(
            int page = 1,
            int? pageSize = null,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object?>>[]? includes);

        Task<ReturnResponse<int>> GetAllCountAsync(
            int page = 1,
            int? pageSize = null,
            Expression<Func<TEntity, bool>>? predicate = null);

        Task<ReturnResponse<TEntity?>> GetByIdAsync(int id);
        Task<ReturnResponse> InsertAsync(TEntity entity);
        Task<ReturnResponse> UpdateAsync(TEntity entity);
        Task<ReturnResponse> DeleteAsync(TEntity entity);
    }
}
