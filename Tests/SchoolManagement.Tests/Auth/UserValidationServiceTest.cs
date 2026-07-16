using Moq;
namespace SchoolManagement.Tests.Auth
{
    // Test class for UserValidationService — verifies login and lockout behaviour.
    public class UserValidationServiceTest
    {
        // Mock<T>: a fake implementation of the interface T so we can control its behaviour without a real database.
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<SchoolManagement.Core.Shared.Contracts.IPasswordHasher> _passwordHasherMock;
        // The real service under test, wired up with the fakes above.
        private readonly UserValidationService _userValidationService;

        // Constructor runs before every test — sets up fresh mocks and a new service instance each time.
        public UserValidationServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>(); // creates a new mock object
            _passwordHasherMock = new Mock<SchoolManagement.Core.Shared.Contracts.IPasswordHasher>();
            // .Object: extracts the actual fake instance that implements the interface.
            _userValidationService = new UserValidationService(_userRepositoryMock.Object, _passwordHasherMock.Object);
        }

        // [Fact]: marks this method as a single test with no parameters.
        [Fact]
        public async Task Login_ValidCredentials_ReturnsUser()
        {
            string username = "admin";
            string password = "admin";
            // Create a User object with the data the mock repository will return.
            User user = new()
            {
                Username = username,
                PasswordHash = "hashed_admin",
                RoleId = 1
            };

            // Arrange: tell the mock how to respond when specific methods are called.
            _userRepositoryMock
                // .Setup: defines what to return when GetUserAsync("admin") is called.
                .Setup(r => r.GetUserAsync("admin"))
                // .ReturnsAsync: makes the mock return the value asynchronously (wraps it in a Task).
                .ReturnsAsync(user);
            _passwordHasherMock
                .Setup(p => p.ComparePassword(password, "hashed_admin"))
                // .Returns (sync): password comparison is not async, so Returns is used.
                .Returns(true);

            // Act: call the method being tested and capture its result.
            ReturnResponse<User> result = await _userValidationService.ValidateUserAsync(username, password);

            // Assert.NotNull: checks that result.Value is not null (a user was returned).
            Assert.NotNull(result.Value);
            // Assert.Equal: checks that the expected value equals the actual value.
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
                // Setup the wrong password — it returns false, simulating a bad login.
                .Setup(p => p.ComparePassword(wrongPassword, "hashed_admin"))
                .Returns(false);
            // Pass the wrong password so the service should reject the login.
            ReturnResponse<User> result = await _userValidationService.ValidateUserAsync(username, wrongPassword);
            // Assert.Equal: the status should be Failed when credentials are wrong.
            Assert.Equal(Status.Failed, result.Status);
        }

        [Fact]
        public async Task Login_Exceeded_Failed_Attempts()
        {
            string username = "admin";
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
                .Returns(false); // always returns false — simulates repeated wrong password

            bool isRejected = false;

            // Loop 7 times to exceed the allowed failure threshold.
            for (int i = 0; i <= 6; i++)
            {
                ReturnResponse<User> result = await _userValidationService.ValidateUserAsync(username, "123");
                // Once the service switches to Rejected status, record it and stop the loop.
                if (result.Status == Status.Rejected)
                {
                    isRejected = true;
                    break;
                }
            }

            // Assert.True: verifies the account was eventually locked out (Rejected).
            Assert.True(isRejected);
        }
    }
}


