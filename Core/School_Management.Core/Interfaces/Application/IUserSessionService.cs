using School_Management.Core.Models;

namespace School_Management.Core.Interfaces.Application
{
    public interface IUserSessionService
    {
        public User? CurrentUser { get; }
        public bool IsLoggedIn => CurrentUser != null;
        public DateTime? LoggedInAt { get; }
        public Task SetSession(User user);
        public Task SetSession(int userId);
        public Task LogoutSession();
    }
}
