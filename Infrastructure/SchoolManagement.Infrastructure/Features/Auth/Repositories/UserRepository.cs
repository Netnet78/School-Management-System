using Microsoft.EntityFrameworkCore;
using SchoolManagement.Core.Features.Auth.Models;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Auth.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(SchoolDbContext context) : base(context)
    {
    }

    protected override IQueryable<User> CreateQuery()
    {
        return Set.Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .Include(u => u.Employee)
            .ThenInclude(e => e != null ? e.Department : null!);
    }

    public override async Task<User?> GetByIdAsync(int id)
    {
        return await CreateQuery().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task CreateUserAsync(string username, string plainPassword, string role = "user")
    {
        string hashedPassword = plainPassword.ToHashedPassword();

        User user = new()
        {
            Username = username,
            PasswordHash = hashedPassword,
            Role = await Context.Roles.FirstAsync(r => r.Name == role)
        };

        await Context.AddAsync(user);
        await SaveAsync();
    }

    public async Task<User?> GetUserAsync(string name)
    {
        return await CreateQuery().FirstOrDefaultAsync(u => u.Username == name);
    }

    public async Task<User?> GetUserByEmployeeIdAsync(int employeeId)
    {
        return await CreateQuery().FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
    }

    public async Task<User> CreateUserForEmployeeAsync(int employeeId, string username, string plainPassword, int roleId)
    {
        string hashedPassword = plainPassword.ToHashedPassword();

        User user = new()
        {
            Username = username,
            PasswordHash = hashedPassword,
            RoleId = roleId,
            EmployeeId = employeeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await Context.AddAsync(user);
        await SaveAsync();
        return user;
    }
}
