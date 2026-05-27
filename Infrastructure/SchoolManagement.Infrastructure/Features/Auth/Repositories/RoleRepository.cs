using Microsoft.EntityFrameworkCore;
using SchoolManagement.Core.Features.Auth.Models;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Auth.Repositories;

public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(SchoolDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await Set.FirstOrDefaultAsync(r => r.Name == name);
    }
}
