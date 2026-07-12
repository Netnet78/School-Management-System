namespace SchoolManagement.Core.Shared.Contracts
{
    public interface IPasswordHasher
    {
        string ToHashedPassword(string originalPassword);
        bool ComparePassword(string originalPassword, string hashedPassword);
    }
}
