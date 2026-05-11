using SchoolManagement.Core.Models;

namespace SchoolManagement.Core.Infrastructure.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        public Task CreateUserAsync(string username, string plainPassword, string role = "User");
        public Task<User?> GetUserAsync(string name);
    }
}
