using SchoolManagement.Core.Models;
using System.Security;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Shared.Models;
using SchoolManagement.Core.Shared.Helpers;
using SchoolManagement.Core.Shared.Time;

namespace SchoolManagement.Application.Services
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
            return await ValidateUserAsync(username, password.ToUnsecureString());
        }

        public async Task<ReturnResponse<User>> ValidateUserAsync(string username, string password)
        {
            await Task.Delay(1000);
            User? user = await _repo.GetUserAsync(username);

            if (user != null && user.LockedOutEnd != null && user.LockedOutEnd > DateTime.UtcNow)
            {
                DateTime lockedOutEnd = TimeHelper.ToLocalTimeZone(user.LockedOutEnd.Value);
                string time = lockedOutEnd.TimeOfDay <= new TimeSpan(12, 0, 0) ? "ព្រឹក" : "ល្ងាច";
                return new()
                {
                    Status = Status.Rejected,
                    Message = $"ការចូលប្រើប្រាស់របស់អ្នក ត្រូវបានបិទ/ផ្អាកជាមុនសិន!\nម៉ោងដែលត្រូវបើកវិញ៖ ម៉ោង {lockedOutEnd.Hour}, {lockedOutEnd.Minute} នាទី {time} \nរយៈពេល៖ {(user.LockedOutEnd.Value - DateTime.UtcNow).TotalMinutes} នាទី",
                };
            }

            bool isValidPassword = user != null && password.ComparePassword(user.PasswordHash) == true;

            if (user != null && isValidPassword)
            {
                user.FailedLoginAttempts = 0;
                await _repo.UpdateAsync(user);
                return new()
                {
                    Status = Status.Success,
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

                await _repo.UpdateAsync(user);
            }

            return new()
            {
                Status = Status.Failed,
                Message = "ព័ត៌មាន Username ឬ Password មិនត្រឹមត្រូវទេ សូមព្យាយាមម្ដងទៀត!",
            };
        }
    }
}
