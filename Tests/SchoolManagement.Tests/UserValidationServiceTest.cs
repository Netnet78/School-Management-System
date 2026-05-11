using Moq;
using SchoolManagement.Application.Services;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Shared.Models;
using SchoolManagement.Core.Shared.Helpers;
using SchoolManagement.Application.Services;

namespace SchoolManagement.Tests
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
            Assert.Equal(Status.Failed, result.Status);
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
                if (result.Status == Status.Rejected)
                {
                    isRejected = true;
                    break;
                }
            }

            Assert.True(isRejected);
        }
    }
}
