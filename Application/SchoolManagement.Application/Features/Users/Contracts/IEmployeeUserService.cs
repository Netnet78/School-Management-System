namespace SchoolManagement.Application.Features.Users.Contracts
{
    public interface IEmployeeUserService
    {
        Task<ReturnResponse<User?>> GetUserByEmployeeAsync(int employeeId);
        Task<ReturnResponse<User>> CreateUserForEmployeeAsync(int employeeId, string username, string password, int roleId);
        Task<ReturnResponse> UpdateUserRoleAsync(int userId, int roleId);
        Task<ReturnResponse> ResetPasswordAsync(int userId, string newPassword);
        Task<ReturnResponse> ToggleUserActiveStatusAsync(int userId);
        Task<ReturnResponse> UnlockUserAsync(int userId);
        Task<ReturnResponse> DeleteUserAsync(int userId);
    }
}
