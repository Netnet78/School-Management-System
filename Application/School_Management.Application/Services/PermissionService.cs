using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class PermissionService : CrudServiceBase<Permission>, IPermissionService
    {
        public PermissionService(IPermissionRepository repository) : base(repository)
        {

        }
    }
}
