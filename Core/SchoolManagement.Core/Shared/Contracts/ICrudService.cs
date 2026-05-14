using SchoolManagement.Core.Shared.Models;
using System.Linq.Expressions;

namespace SchoolManagement.Core.Shared.Contracts
{
    public interface ICrudService<TEntity>
    {
        Task<ReturnResponse<IEnumerable<TEntity>>> GetAllAsync(
            int page = 1,
            int? pageSize = null,
            IEnumerable<FilterCondition<TEntity>>? filters = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object?>>[]? includes);
        Task<ReturnResponse<IEnumerable<TEntity>>> GetAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            int page = 1,
            int? pageSize = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object?>>[]? includes);

        Task<ReturnResponse<int>> GetAllCountAsync(
            int page = 1,
            int? pageSize = null,
            IEnumerable<FilterCondition<TEntity>>? filters = null);

        Task<ReturnResponse<TEntity?>> GetByIdAsync(int id);
        Task<ReturnResponse> InsertAsync(TEntity entity);
        Task<ReturnResponse> UpdateAsync(TEntity entity);
        Task<ReturnResponse> DeleteAsync(TEntity entity);
    }
}
