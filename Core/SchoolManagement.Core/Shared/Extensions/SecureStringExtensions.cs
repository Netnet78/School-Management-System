using System.Runtime.InteropServices;
using System.Security;

namespace SchoolManagement.Core.Shared.Extensions
{
    public static class SecureStringExtensions
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
    }
}
