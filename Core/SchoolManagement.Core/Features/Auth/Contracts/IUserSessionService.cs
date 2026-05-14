

using SchoolManagement.Core.Features.Auth.Models;

namespace SchoolManagement.Core.Features.Auth.Contracts
{
    public interface IUserSessionService
    {
        public event Action<User?>? OnUserSessionChanged;
        public event Action<User>? OnUserLoggedOut;
        public User? CurrentUser { get; }
        public bool IsLoggedIn => CurrentUser != null;
        public DateTime? LoggedInAt { get; }
        public Task SetSession(User user);
        public Task SetSession(int userId);
        public Task LogoutSession();
    }
}

