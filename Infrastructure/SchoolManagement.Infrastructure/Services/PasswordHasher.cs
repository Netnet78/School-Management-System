using SchoolManagement.Core.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string ToHashedPassword(string originalPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(originalPassword);
        }

        public bool ComparePassword(string originalPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(originalPassword, hashedPassword);
        }
    }
}
