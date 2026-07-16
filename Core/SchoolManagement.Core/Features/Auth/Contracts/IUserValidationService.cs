using System.Security;
using SchoolManagement.Core.Features.Auth.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Core.Features.Auth.Contracts;

public interface IUserValidationService
{
    Task<ReturnResponse<User>> ValidateUserAsync(string username, SecureString password);
    Task<ReturnResponse<User>> ValidateUserAsync(string username, string password);
}
