using System.Runtime.InteropServices;

namespace SchoolManagement.Core.Shared.Time
{
    public static class TimeHelper
    {
        /// <summary>
        /// Convert the UTC timezone into Cambodia timezone (UTC+7)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ToLocalTimeZone(this DateTime dateTime)
        {
            string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "SE Asia Standard Time"
                : "Asia/Phnom_Penh";

            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, tz);

            return localTime;
        }

        public static DateTime ToUtcTimeZone(this DateTime dateTime)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTime);
            return utcTime;
        }
    }
}
