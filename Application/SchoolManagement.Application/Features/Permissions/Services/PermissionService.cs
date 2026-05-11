using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class PermissionService : CrudServiceBase<Permission>, IPermissionService
    {
        public PermissionService(IPermissionRepository repository) : base(repository)
        {

        }
    }
}
