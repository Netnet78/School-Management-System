using System.Runtime.InteropServices;

namespace School_Management.Core.Helpers
{
    public static class TimeHelper
    {
        public static DateTime ToLocalTimeZone(this DateTime dateTime)
        {
            string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "SE Asia Standard Time"
                : "Asia/Phnom_Penh";

            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, tz);

            return localTime;
        }
    }
}
