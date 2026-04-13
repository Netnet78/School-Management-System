using Microsoft.EntityFrameworkCore;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories
{

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
            string hashedPassword = plainPassword.ToHashedPassword();

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

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null) _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
