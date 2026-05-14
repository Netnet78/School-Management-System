
using SchoolManagement.Core.Features.AuditLogs.Models;
using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Core.Features.AuditLogs.Contracts
{
    public interface IAuditLogService : ICrudService<AuditLog>
    {
    }
}

