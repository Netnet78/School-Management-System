using SchoolManagement.Core.Shared.Contracts;
using SchoolManagement.Infrastructure.Shared.Contracts;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace SchoolManagement.Application.Features.Shared.Services
{
    public abstract class CrudServiceBase<TEntity> : ICrudService<TEntity>
        where TEntity : class, IEntity
    {
        private readonly IBaseRepository<TEntity> _repository;
        private readonly IAuthorizationService? _authorizationService;

        protected CrudServiceBase(IBaseRepository<TEntity> repository)
        {
            _repository = repository;
        }

        protected CrudServiceBase(IBaseRepository<TEntity> repository,
            IAuthorizationService authorizationService)
        {
            _repository = repository;
            _authorizationService = authorizationService;
        }

        protected virtual PermissionType? ViewPermission => null;
        protected virtual PermissionType? InsertPermission => null;
        protected virtual PermissionType? EditPermission => null;
        protected virtual PermissionType? DeletePermission => null;

        protected virtual IEnumerable<FilterCondition<TEntity>> ApplyAccessFilters(
            User user, IEnumerable<FilterCondition<TEntity>>? filters) =>
            filters ?? [];

        public virtual async Task<ReturnResponse<IEnumerable<TEntity>>> GetAllAsync(
            int page = 1,
            int? pageSize = null,
            IEnumerable<FilterCondition<TEntity>>? filters = null,
            IEnumerable<SortCriteria<TEntity>>? orderBy = null,
            params string[]? includes)
        {
            try
            {
                if (_authorizationService != null && ViewPermission is { } permission)
                {
                    Type entityType = typeof(TEntity);
                    if (!await CanProceed(permission)) return new()
                    {
                        Status = Status.Rejected,
                        Message = $"អ្នកគ្មានសិទ្ធិក្នុងការទាញយកទិន្នន័យ " +
                        $"{entityType.GetCustomAttribute<DescriptionAttribute>()} " +
                        $"បានឡើយ"
                    };
                }

                IEnumerable<TEntity> entities = await _repository.FindAsync(filters, page, pageSize, orderBy, includes);
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
                    Message = $"មិនអាចទាញយកទិន្នន័យ {typeof(TEntity).Name} បានឡើយ។\n{ex.Message}",
                };
            }
        }

        public virtual async Task<ReturnResponse<int>> GetAllCountAsync(
            int page = 1,
            int? pageSize = null,
            IEnumerable<FilterCondition<TEntity>>? filters = null)
        {
            try
            {
                if (_authorizationService != null && ViewPermission is { } permission)
                {
                    Type entityType = typeof(TEntity);
                    if (!await CanProceed(permission)) return new()
                    {
                        Status = Status.Rejected,
                        Message = $"អ្នកគ្មានសិទ្ធិក្នុងការរាប់ចំនួនទិន្នន័យ " +
                        $"{entityType.GetCustomAttribute<DescriptionAttribute>()} " +
                        $"បានឡើយ"
                    };
                }

                int count = await _repository.CountAsync(filters, page, pageSize);
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
                if (_authorizationService != null && ViewPermission is { } permission)
                {
                    Type entityType = typeof(TEntity);
                    if (!await CanProceed(permission)) return new()
                    {
                        Status = Status.Rejected,
                        Message = $"អ្នកគ្មានសិទ្ធិចូលមើលទិន្នន័យ " +
                        $"{entityType.GetCustomAttribute<DescriptionAttribute>()} " +
                        $"ណាមួយបានឡើយ"
                    };
                }

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

        public virtual async Task<ReturnResponse> InsertAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                if (_authorizationService != null && InsertPermission is { } permission)
                {
                    Type entityType = typeof(TEntity);
                    if (!await CanProceed(permission)) return new()
                    {
                        Status = Status.Rejected,
                        Message = $"អ្នកគ្មានសិទ្ធិបន្ថែមទិន្នន័យ" +
                        $"{entityType.GetCustomAttribute<DescriptionAttribute>()}" +
                        $"បានឡើយ"
                    };
                }

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

        public virtual async Task<ReturnResponse> UpdateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                if (_authorizationService != null && DeletePermission is { } permission)
                {
                    Type entityType = typeof(TEntity);
                    if (!await CanProceed(permission)) return new()
                    {
                        Status = Status.Rejected,
                        Message = $"អ្នកគ្មានសិទ្ធិក្នុងការកែប្រែទិន្នន័យ" +
                        $"{entityType.GetCustomAttribute<DescriptionAttribute>()} " +
                        $"បានឡើយ"
                    };
                }

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

        public virtual async Task<ReturnResponse> DeleteAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                if (_authorizationService != null && DeletePermission is { } permission)
                {
                    Type entityType = typeof(TEntity);
                    if (!await CanProceed(permission)) return new()
                    {
                        Status = Status.Rejected,
                        Message = $"អ្នកគ្មានសិទ្ធិក្នុងការលុបទិន្នន័យ" +
                        $"{entityType.GetCustomAttribute<DescriptionAttribute>()}" +
                        $"បានឡើយ"
                    };
                }

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

        private async Task<bool> CanProceed(params PermissionType[] requiredPermissions)
        {
            if (_authorizationService == null) throw new 
                    InvalidOperationException("Cannot proceed due to the authorization service couldn't be found!");

            User? user = _authorizationService.CurrentUser;
            if (user == null) return false;

            ReturnResponse result = await _authorizationService.AuthorizeAsync(null, OperatorMode.AND, requiredPermissions);
            return result.Status == Status.Success;
        }
    }
}

