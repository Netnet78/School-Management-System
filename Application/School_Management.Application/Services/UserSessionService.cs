using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUserRepository _userRepository;

        public event Action<User?>? OnUserSessionChanged;
        public event Action<User>? OnUserLoggedOut;
        public User? CurrentUser { get; private set; }
        public DateTime? LoggedInAt { get; private set; }

        public UserSessionService(IAuditLogRepository auditLogRepository, IUserRepository userRepository)
        {
            _auditLogRepository = auditLogRepository;
            _userRepository = userRepository;
        }

        public async Task LogoutSession()
        {
            if (CurrentUser == null) return;

            DateTime logoutTime = DateTime.UtcNow;

            await _auditLogRepository.AddAsync(new()
            {
                CustomDescription = $"User {CurrentUser.Username} LOGGED OUT after {(logoutTime - LoggedInAt)?.TotalMinutes:F1} minutes",
                Timestamp = logoutTime,
                UserId = CurrentUser.Id
            });

            OnUserLoggedOut?.Invoke(CurrentUser);
            OnUserSessionChanged?.Invoke(null);
        }

        public async Task SetSession(User user)
        {
            await SetSession(user.Id);
        }

        public async Task SetSession(int userId)
        {
            User? user = await _userRepository.GetByIdAsync(userId);

            if (user != null)
            {
                CurrentUser = user;
                LoggedInAt = DateTime.UtcNow;
                CurrentUser.LastLogin = LoggedInAt;

                await _auditLogRepository.AddAsync(new()
                {
                    CustomDescription = $"User {CurrentUser.Username} has LOGGED IN",
                    Timestamp = LoggedInAt.Value,
                    UserId = CurrentUser.Id
                });

                OnUserSessionChanged?.Invoke(user);
                await _userRepository.UpdateAsync(CurrentUser);
            }
            else
            {
                throw new ArgumentException($"There's no existing user in the database with the ID of \"{userId}\"");
            }
        }
    }
}
