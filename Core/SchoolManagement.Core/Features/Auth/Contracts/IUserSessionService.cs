using SchoolManagement.Core.Features.Auth.Models;

namespace SchoolManagement.Core.Features.Auth.Contracts;

public interface IUserSessionService
{
    User? CurrentUser { get; }
    event Action<User?>? OnUserSessionChanged;
    Task SetSession(User user);
    Task SetSession(int userId);
    Task LogoutSession();
}
