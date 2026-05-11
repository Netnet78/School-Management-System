using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;

namespace SchoolManagement.Application.Services
{
    public class RoleService : CrudServiceBase<Role>, IRoleService
    {
        public RoleService(IRoleRepository repository) : base(repository)
        {
        }
    }
}
