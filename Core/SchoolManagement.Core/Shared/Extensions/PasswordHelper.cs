using System.Runtime.InteropServices;
using System.Security;

namespace SchoolManagement.Core.Shared.Extensions
{
    public static class PasswordHelper
    {
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
            return BCrypt.Net.BCrypt.HashPassword(originalPassword);
        }

        public static bool ComparePassword(this string originalPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(originalPassword, hashedPassword);
        }
    }
}
