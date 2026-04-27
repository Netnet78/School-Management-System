using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Infrastructure
{
    public interface IUserRepository : IBaseRepository<User>
    {
        public Task CreateUserAsync(string username, string plainPassword, string role = "User");
        public Task<User?> GetUserAsync(string name);
    }
}
