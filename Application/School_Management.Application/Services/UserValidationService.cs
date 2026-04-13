using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using System.Security;
using School_Management.Core.Helpers;
using School_Management.Core.Enums;

namespace School_Management.Application.Services
{
    public class UserValidationService : IUserValidationService
    {
        private readonly IUserRepository _repo;
        private readonly int maximumAttempts = 5;
        public UserValidationService(IUserRepository repo)
        {
            _repo = repo;
        }
        public async Task<ReturnResponse<User>> ValidateUserAsync(string username, SecureString password)
        {
            await Task.Delay(1000);
            User? user = await _repo.GetUserAsync(username);

            if (user != null && user.LockedOutEnd > DateTime.UtcNow)
            {
                DateTime lockedOutEnd = user.LockedOutEnd.Value;
                string time = lockedOutEnd.TimeOfDay <= new TimeSpan(12, 0, 0) ? "ព្រឹក" : "ល្ងាច";
                return new()
                {
                    Status = ReturnStatus.Rejected,
                    Message = $"ការចូលប្រើប្រាស់របស់អ្នក ត្រូវបានបិទ/ផ្អាកជាមុនសិន!\nម៉ោងដែលត្រូវបើកវិញ៖ ម៉ោង {lockedOutEnd.Hour}, {lockedOutEnd.Minute} នាទី {time} \nរយៈពេល៖ {user.LockedOutEnd}",
                };
            }

            string unsecuredPassword = password.ToUnsecureString();
            bool isValidPassword = user == null ? false : unsecuredPassword.ComparePassword(user.PasswordHash) == true;

            if (user != null && isValidPassword)
            {
                user.FailedLoginAttempts = 0;
                return new()
                {
                    Status = ReturnStatus.Success,
                    Value = user,
                };
            }

            if (user != null && !isValidPassword)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= maximumAttempts)
                {
                    user.LockedOutEnd = DateTime.UtcNow.AddMinutes(5);
                }
            }

            return new()
            {
                Status = ReturnStatus.Failed,
                Message = "ព័ត៌មាន Username ឬ Password មិនត្រឹមត្រូវទេ សូមព្យាយាមម្ដងទៀត!"
            };
        }

        public async Task<ReturnResponse<User>> ValidateUserAsync(string username, string password)
        {
            await Task.Delay(1000);
            User? user = await _repo.GetUserAsync(username);

            if (user != null && user.LockedOutEnd > DateTime.UtcNow)
            {
                DateTime lockedOutEnd = user.LockedOutEnd.Value;
                string time = lockedOutEnd.TimeOfDay <= new TimeSpan(12, 0, 0) ? "ព្រឹក" : "ល្ងាច";
                return new()
                {
                    Status = ReturnStatus.Rejected,
                    Message = $"ការចូលប្រើប្រាស់របស់អ្នក ត្រូវបានបិទ/ផ្អាកជាមុនសិន!\nម៉ោងដែលត្រូវបើកវិញ៖ ម៉ោង {lockedOutEnd.Hour}, {lockedOutEnd.Minute} នាទី {time} \nរយៈពេល៖ {user.LockedOutEnd}",
                };
            }

            bool isValidPassword = user == null ? false : password.ComparePassword(user.PasswordHash) == true;

            if (user != null && isValidPassword)
            {
                user.FailedLoginAttempts = 0;
                return new()
                {
                    Status = ReturnStatus.Success,
                    Value = user,
                };
            }

            if (user != null && !isValidPassword)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= maximumAttempts)
                {
                    user.LockedOutEnd = DateTime.UtcNow.AddMinutes(5);
                }
            }

            return new()
            {
                Status = ReturnStatus.Failed,
                Message = "ព័ត៌មាន Username ឬ Password មិនត្រឹមត្រូវទេ សូមព្យាយាមម្ដងទៀត!",
            };
        }
    }
}
