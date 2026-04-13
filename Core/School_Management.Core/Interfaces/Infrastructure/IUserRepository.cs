using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IUserRepository
    {
        public Task CreateUserAsync(string username, string plainPassword, string role = "User");
        public Task UpdateUserAsync(User user);
        public Task DeleteUserAsync(int userId);
        public Task<IEnumerable<User>> GetAllUsersAsync();
        public Task<User?> GetUserAsync(int id);
        public Task<User?> GetUserAsync(string name);
    }
}
