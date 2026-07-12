using Moq;
namespace SchoolManagement.Tests.Auth
{
    public class UserValidationServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<SchoolManagement.Core.Shared.Contracts.IPasswordHasher> _passwordHasherMock;
        private readonly UserValidationService _userValidationService;
        public UserValidationServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<SchoolManagement.Core.Shared.Contracts.IPasswordHasher>();
            _userValidationService = new UserValidationService(_userRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsUser()
        {
            string username = "admin";
            string password = "admin";
            User user = new()
            {
                Username = username,
                PasswordHash = "hashed_admin",
                RoleId = 1
            };

            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetUserAsync("admin"))
                .ReturnsAsync(user);
            _passwordHasherMock
                .Setup(p => p.ComparePassword(password, "hashed_admin"))
                .Returns(true);

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
                PasswordHash = "hashed_admin",
                RoleId = 1
            };
            _userRepositoryMock
                .Setup(r => r.GetUserAsync("admin"))
                .ReturnsAsync(user);
            _passwordHasherMock
                .Setup(p => p.ComparePassword(wrongPassword, "hashed_admin"))
                .Returns(false);
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
                PasswordHash = "hashed_admin",
                RoleId = 1,
            };
            _userRepositoryMock
                .Setup(r => r.GetUserAsync("admin"))
                .ReturnsAsync(user);
            _passwordHasherMock
                .Setup(p => p.ComparePassword("123", "hashed_admin"))
                .Returns(false);

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

