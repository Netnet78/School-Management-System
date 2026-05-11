using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class AuditLogService : CrudServiceBase<AuditLog>, IAuditLogService
    {
        public AuditLogService(IAuditLogRepository repository) : base(repository)
        {
        }
    }
}
