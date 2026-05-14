using Microsoft.EntityFrameworkCore;
using SchoolManagement.Core.Features.Auth.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(SchoolDbContext context) : base(context)
    {
    }

    protected override IQueryable<User> CreateQuery()
    {
        return Set
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .Include(u => u.Employee);
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
}
