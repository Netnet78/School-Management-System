using System.Runtime.InteropServices;
using System.Security;

namespace SchoolManagement.Core.Shared.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Converts the value of a SecureString instance to its unencrypted string representation.
        /// </summary>
        /// <remarks>The returned string is stored in memory in plain text and is not protected. Use
        /// caution to avoid exposing sensitive information, and clear the string from memory as soon as it is no longer
        /// needed.</remarks>
        /// <param name="secureString">The SecureString instance to convert. Cannot be null.</param>
        /// <returns>A string containing the unencrypted contents of the specified SecureString.</returns>
        public static string ToUnsecureString(this SecureString secureString)
        {
            IntPtr pointer = Marshal.SecureStringToBSTR(secureString);
            try
            {
                return Marshal.PtrToStringBSTR(pointer);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(pointer);
            }
        }
        public static string ToHashedPassword(this string originalPassword)
        {
            string password = BCrypt.Net.BCrypt.HashPassword(originalPassword);
            return password;
        }
        public static bool ComparePassword(this string originalPassword, string hashedPassword)
        {
            bool isValid = BCrypt.Net.BCrypt.Verify(originalPassword, hashedPassword);
            return isValid;
        }
    }
}
