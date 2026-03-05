using School_Management.Core.Models;
using School_Management.Infrastructure.Repositories;


namespace School_Management.Application.Services
{
    public interface IUserValidationService
    {
        public Task<User> ValidateUserAsync(string username, string password);
    }

    public class UserValidationService : IUserValidationService
    {
        private readonly IUserRepository _repo;
        public UserValidationService(IUserRepository repo)
        {
            _repo = repo;
        }
        public async Task<User> ValidateUserAsync(string username, string password)
        {
            User? user = await _repo.GetUserAsync(username);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }
            throw new Exception("Invalid username or password");
        }
    }
}
