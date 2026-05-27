
using SchoolManagement.Core.Features.Auth.Models;
using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Auth.Contracts
{
    public interface IPermissionRepository : IBaseRepository<Permission>
    {
    }
}
