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

    public async Task UpdateRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds)
    {
        Role? role = await Set
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null) return;

        role.Permissions.Clear();

        List<Permission> permissions = await Context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync();

        foreach (Permission permission in permissions)
        {
            role.Permissions.Add(permission);
        }

        await Context.SaveChangesAsync();
    }
}
