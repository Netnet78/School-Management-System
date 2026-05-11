using Microsoft.EntityFrameworkCore;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

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
