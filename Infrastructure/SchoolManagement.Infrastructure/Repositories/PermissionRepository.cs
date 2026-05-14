using SchoolManagement.Core.Features.Auth.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(SchoolDbContext context) : base(context)
    {
    }
}
