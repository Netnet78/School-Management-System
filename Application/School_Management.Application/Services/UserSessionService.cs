using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUserRepository _userRepository;
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

            await _auditLogRepository.AddAsync(new()
            {
                Action = $"User {CurrentUser.Username} has LOGGED OUT",
                Timestamp = DateTime.Now,
                UserId = CurrentUser.Id
            });

            CurrentUser = null;
            LoggedInAt = null;
        }

        public async Task SetSession(User user)
        {
            CurrentUser = user;
            LoggedInAt = DateTime.Now;

            await _auditLogRepository.AddAsync(new()
            {
                Action = $"User {CurrentUser.Username} has LOGGED IN",
                Timestamp = (DateTime)LoggedInAt,
                UserId = CurrentUser.Id
            });
        }

        public async Task SetSession(int userId)
        {
            User? user = await _userRepository.GetUserAsync(userId);

            if (user != null)
            {
                CurrentUser = user;
                LoggedInAt = DateTime.Now;

                await _auditLogRepository.AddAsync(new()
                {
                    Action = $"User {CurrentUser.Username} has LOGGED IN",
                    Timestamp = (DateTime)LoggedInAt,
                    UserId = CurrentUser.Id
                });
            }
            else
            {
                throw new ArgumentException($"There's no existing user in the database with the ID of \"{userId}\"");
            }
        }
    }
}
