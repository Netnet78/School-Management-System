namespace SchoolManagement.Core.Shared.Extensions
{
    /// <summary>
    /// Provides extension methods for splitting and concatenating names represented as strings or tuples.
    /// </summary>
    /// <remarks>This static class offers utility methods for handling names in the format of last name and
    /// first name. The methods are intended to simplify common operations such as splitting a full name into its
    /// components or combining them into a single string. All methods are static and can be used as extension methods
    /// where applicable.</remarks>
    public static class NameExtension
    {
        public static (string LastName, string FirstName) SplitName(this string fullName)
        {
            ReadOnlySpan<char> remaining = fullName.AsSpan();
            int spaceIndex = remaining.IndexOf(' ');

            string lastName = spaceIndex >= 0 ? remaining[..spaceIndex].ToString() : fullName;
            string firstName = spaceIndex >= 0 ? remaining[(spaceIndex + 1)..].Trim().ToString() : "";

            return (lastName, firstName);
        }

        public static string ConcateName(this (string LastName, string FirstName) fullName)
        {
            return ConcateName(fullName.FirstName, fullName.LastName);
        }

        public static string ConcateName(string lastName, string firstName)
        {
            return $"{lastName} {firstName}";
        }
    }
}
