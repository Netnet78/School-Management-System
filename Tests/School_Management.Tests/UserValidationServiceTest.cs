using Moq;
using School_Management.Application.Services;
using School_Management.Core.Models;
using School_Management.Infrastructure.Repositories;

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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = 1
            };

            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetUserAsync("admin"))
                .ReturnsAsync(user);

            User result = await _userValidationService.ValidateUserAsync(username, password);

            Assert.NotNull(result);
            Assert.Equal("admin", result.Role.Name);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ThrowsException()
        {
            string username = "admin";
            string password = "admin";
            User user = new()
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = 1
            };
            _userRepositoryMock
                .Setup(r => r.GetUserAsync("admin"))
                .ReturnsAsync(user);
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                await _userValidationService.ValidateUserAsync("admin", "wrongpassword"));
            Assert.Equal("Invalid username or password", ex.Message);
        }
    }
}
