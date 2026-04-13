using School_Management.Core.Models;
using System.Security;

namespace School_Management.Core.Interfaces.Application
{
    public interface IUserValidationService
    {
        public Task<ReturnResponse<User>> ValidateUserAsync(string username, string password);
        public Task<ReturnResponse<User>> ValidateUserAsync(string username, SecureString password);
    }
}
