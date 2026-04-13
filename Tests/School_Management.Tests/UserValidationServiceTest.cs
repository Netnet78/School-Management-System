using Moq;
using School_Management.Application.Services;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Core.Enums;

namespace School_Management.Tests
{
    public class UserValidationServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserValidationService _userValidationService;
        public UserValidationServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userValidationService = new UserValidationService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsUser()
        {
            string username = "admin";
            string password = "admin";
            User user = new()
            {
                Username = username,
                PasswordHash = password.ToHashedPassword(),
                RoleId = 1
            };

            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetUserAsync("admin"))
                .ReturnsAsync(user);

            ReturnResponse<User> result = await _userValidationService.ValidateUserAsync(username, password);

            Assert.NotNull(result.Value);
            Assert.Equal("admin", result.Value.Username);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ThrowsException()
        {
            string username = "admin";
            string password = "admin";
            string wrongPassword = "wrongwrongwrong";
            User user = new()
            {
                Username = username,
                PasswordHash = password.ToHashedPassword(),
                RoleId = 1
            };
            _userRepositoryMock
                .Setup(r => r.GetUserAsync("admin"))
                .ReturnsAsync(user);
            ReturnResponse<User> result = await _userValidationService.ValidateUserAsync(username, wrongPassword);
            Assert.Equal(ReturnStatus.Failed, result.Status);
        }

        [Fact]
        public async Task Login_Exceeded_Failed_Attempts()
        {
            string username = "admin";
            string password = "admin";
            User user = new()
            {
                Username = username,
                PasswordHash = password.ToHashedPassword(),
                RoleId = 1,
            };
            _userRepositoryMock
                .Setup(r => r.GetUserAsync("admin"))
                .ReturnsAsync(user);

            bool isRejected = false;

            for (int i = 0; i <= 6; i++)
            {
                ReturnResponse<User> result = await _userValidationService.ValidateUserAsync(username, "123");
                if (result.Status == ReturnStatus.Rejected)
                {
                    isRejected = true;
                    break;
                }
            }

            Assert.True(isRejected);
        }
    }
}
