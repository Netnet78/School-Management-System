using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        public Task CreateUserAsync(string username, string plainPassword, string role="User");
        public Task<IEnumerable<User>> GetAllUsersAsync();
        public Task<User?> GetUserAsync(int id);
        public Task<User?> GetUserAsync(string name);
    }

    public class UserRepository : IUserRepository
    {
        private readonly SchoolDbContext _context;

        public UserRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task CreateUserAsync(string username, string plainPassword, string role = "user")
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            User user = new()
            {
                Username = username,
                PasswordHash = hashedPassword,
                Role = await _context.Roles.FirstAsync(r => r.Name == role)
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserAsync(int id)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<User?> GetUserAsync(string name)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == name);
            return user;
        }
    }
}
