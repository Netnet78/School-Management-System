using System.Linq.Expressions;

namespace SchoolManagement.Application.Features.AuditLogs.Services
{
    public class AuditLogService : CrudServiceBase<AuditLog>, IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository repository) : base(repository)
        {
            _auditLogRepository = repository;
        }

        public async Task<ReturnResponse<IEnumerable<AuditLog>>> GetAllAsync(
            int page,
            int? pageSize,
            IEnumerable<FilterCondition<AuditLog>>? filters,
            Expression<Func<AuditLog, bool>>? extraPredicate,
            IEnumerable<SortCriteria<AuditLog>>? orderBy,
            params string[]? includes)
        {
            try
            {
                IEnumerable<AuditLog> logs = await _auditLogRepository.FindAsync(
                    filters, extraPredicate, page, pageSize, orderBy, includes);

                return new()
                {
                    Status = Status.Success,
                    Value = logs,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"Could not retrieve audit logs.\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse<int>> GetAllCountAsync(
            int page,
            int? pageSize,
            IEnumerable<FilterCondition<AuditLog>>? filters,
            Expression<Func<AuditLog, bool>>? extraPredicate)
        {
            try
            {
                int count = await _auditLogRepository.CountAsync(
                    filters, extraPredicate, page, pageSize);

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
                    Message = $"Could not count audit logs.\n{ex.Message}",
                };
            }
        }
    }
}
