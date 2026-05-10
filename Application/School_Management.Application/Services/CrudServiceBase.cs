using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using System.Linq;
using System.Linq.Expressions;

namespace School_Management.Application.Services
{
    public abstract class CrudServiceBase<TEntity> : ICrudService<TEntity>
        where TEntity : class
    {
        private readonly IBaseRepository<TEntity> _repository;

        protected CrudServiceBase(IBaseRepository<TEntity> repository)
        {
            _repository = repository;
        }

        public async Task<ReturnResponse<IEnumerable<TEntity>>> GetAllAsync(
            int page = 1,
            int? pageSize = null,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object?>>[]? includes)
        {
            try
            {
                IEnumerable<TEntity> entities = await _repository.FindAsync(predicate, page, pageSize, orderBy, includes);
                return new()
                {
                    Status = Status.Success,
                    Value = entities,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Could not retrieve {typeof(TEntity).Name} data.\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse<int>> GetAllCountAsync(
            int page = 1,
            int? pageSize = null,
            Expression<Func<TEntity, bool>>? predicate = null)
        {
            try
            {
                int count = await _repository.CountAsync(predicate, page, pageSize);
                return new()
                {
                    Status = Status.Success,
                    Value = count,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Could not get {typeof(TEntity).Name} count.\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse<TEntity?>> GetByIdAsync(int id)
        {
            try
            {
                TEntity? entity = await _repository.GetByIdAsync(id);
                return new()
                {
                    Status = entity is null ? Status.Rejected : Status.Success,
                    Value = entity,
                    Message = entity is null
                        ? $"Could not find {typeof(TEntity).Name} with ID {id}."
                        : string.Empty,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Could not retrieve {typeof(TEntity).Name}.\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse> InsertAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                await _repository.AddAsync(entity);
                return new()
                {
                    Status = Status.Success,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Could not insert {typeof(TEntity).Name}.\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse> UpdateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                await _repository.UpdateAsync(entity);
                return new()
                {
                    Status = Status.Success,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Could not update {typeof(TEntity).Name}.\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse> DeleteAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                await _repository.DeleteAsync(entity);
                return new()
                {
                    Status = Status.Success,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Could not delete {typeof(TEntity).Name}.\n{ex.Message}",
                };
            }
        }
    }
}
