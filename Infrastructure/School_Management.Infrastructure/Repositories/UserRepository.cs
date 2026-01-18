using Microsoft.EntityFrameworkCore;
using School_Management.Infrastructure.Data;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        public Task CreateUserAsync(string username, string plainPassword, string role="User");
        public Task<User?> ValidateUserAsync(string username, string password);
    }

    public class UserRepository : IUserRepository
    {
        private readonly StudentDbContext _context;

        public UserRepository(StudentDbContext context)
        {
            _context = context;
        }

        public async Task CreateUserAsync(string username, string plainPassword, string role = "User")
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            User user = new()
            {
                Username = username,
                PasswordHash = hashedPassword,
                Role = role
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }

    }
}
