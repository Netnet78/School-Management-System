using SchoolManagement.Core.Features.Auth.Models;
using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Auth.Contracts
{
    public interface IUserRepository : IBaseRepository<User>
    {
        public Task CreateUserAsync(string username, string plainPassword, string role = "User");
        public Task<User?> GetUserAsync(string name);
        public Task<User?> GetUserByEmployeeIdAsync(int employeeId);
        public Task<User> CreateUserForEmployeeAsync(int employeeId, string username, string plainPassword, int roleId);
    }
}
