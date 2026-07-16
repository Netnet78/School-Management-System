namespace SchoolManagement.Application.Features.Shared.Contracts
{
    public interface ICrudService<TEntity>
    {
        Task<ReturnResponse<IEnumerable<TEntity>>> GetAllAsync(
            int page = 1,
            int? pageSize = null,
            IEnumerable<FilterCondition<TEntity>>? filters = null,
            IEnumerable<SortCriteria<TEntity>>? orderBy = null,
            params string[]? includes);

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
