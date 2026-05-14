namespace SchoolManagement.Application.Features.AuditLogs.Services
{
    public class AuditLogService : CrudServiceBase<AuditLog>, IAuditLogService
    {
        public AuditLogService(IAuditLogRepository repository) : base(repository)
        {
        }
    }
}


