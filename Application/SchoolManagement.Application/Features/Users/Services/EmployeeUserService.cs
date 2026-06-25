namespace SchoolManagement.Application.Features.Users.Services
{
    public class EmployeeUserService : IEmployeeUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public EmployeeUserService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<ReturnResponse<User?>> GetUserByEmployeeAsync(int employeeId)
        {
            try
            {
                User? user = await _userRepository.GetUserByEmployeeIdAsync(employeeId);
                return new() { Status = Status.Success, Value = user };
            }
            catch (Exception ex)
            {
                return new() { Status = Status.Failed, Message = ex.Message };
            }
        }

        public async Task<ReturnResponse<User>> CreateUserForEmployeeAsync(int employeeId, string username, string password, int roleId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return new() { Status = Status.Failed, Message = "Username is required." };

                if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
                    return new() { Status = Status.Failed, Message = "Password must be at least 4 characters." };

                User? existingUser = await _userRepository.GetUserAsync(username);
                if (existingUser != null)
                    return new() { Status = Status.Failed, Message = "Username is already taken." };

                User? existingEmployeeUser = await _userRepository.GetUserByEmployeeIdAsync(employeeId);
                if (existingEmployeeUser != null)
                    return new() { Status = Status.Failed, Message = "This employee already has a user account." };

                Role? role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                    return new() { Status = Status.Failed, Message = "Selected role not found." };

                User user = await _userRepository.CreateUserForEmployeeAsync(employeeId, username, password, roleId);
                return new() { Status = Status.Success, Value = user };
            }
            catch (Exception ex)
            {
                return new() { Status = Status.Failed, Message = ex.Message };
            }
        }

        public async Task<ReturnResponse> UpdateUserRoleAsync(int userId, int roleId)
        {
            try
            {
                User? user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return new() { Status = Status.Failed, Message = "User not found." };

                Role? role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                    return new() { Status = Status.Failed, Message = "Role not found." };

                user.RoleId = roleId;
                await _userRepository.UpdateAsync(user);
                return new() { Status = Status.Success };
            }
            catch (Exception ex)
            {
                return new() { Status = Status.Failed, Message = ex.Message };
            }
        }

        public async Task<ReturnResponse> ResetPasswordAsync(int userId, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
                    return new() { Status = Status.Failed, Message = "Password must be at least 4 characters." };

                User? user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return new() { Status = Status.Failed, Message = "User not found." };

                user.PasswordHash = newPassword.ToHashedPassword();
                await _userRepository.UpdateAsync(user);
                return new() { Status = Status.Success };
            }
            catch (Exception ex)
            {
                return new() { Status = Status.Failed, Message = ex.Message };
            }
        }

        public async Task<ReturnResponse> ToggleUserActiveStatusAsync(int userId)
        {
            try
            {
                User? user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return new() { Status = Status.Failed, Message = "User not found." };

                user.IsActive = !user.IsActive;
                await _userRepository.UpdateAsync(user);
                return new() { Status = Status.Success };
            }
            catch (Exception ex)
            {
                return new() { Status = Status.Failed, Message = ex.Message };
            }
        }

        public async Task<ReturnResponse> UnlockUserAsync(int userId)
        {
            try
            {
                User? user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return new() { Status = Status.Failed, Message = "User not found." };

                user.FailedLoginAttempts = 0;
                user.LockedOutEnd = null;
                await _userRepository.UpdateAsync(user);
                return new() { Status = Status.Success };
            }
            catch (Exception ex)
            {
                return new() { Status = Status.Failed, Message = ex.Message };
            }
        }

        public async Task<ReturnResponse> DeleteUserAsync(int userId)
        {
            try
            {
                User? user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return new() { Status = Status.Failed, Message = "User not found." };

                await _userRepository.DeleteAsync(user);
                return new() { Status = Status.Success };
            }
            catch (Exception ex)
            {
                return new() { Status = Status.Failed, Message = ex.Message };
            }
        }
    }
}
