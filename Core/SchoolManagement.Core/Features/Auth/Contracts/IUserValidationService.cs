using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;
using System.Security;

namespace SchoolManagement.Core.Application.Interfaces
{
    public interface IUserValidationService
    {
        public Task<ReturnResponse<User>> ValidateUserAsync(string username, string password);
        public Task<ReturnResponse<User>> ValidateUserAsync(string username, SecureString password);
    }
}
