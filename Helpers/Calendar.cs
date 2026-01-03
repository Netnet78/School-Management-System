
namespace New_Student_Management.Helpers
{
    public interface IKhmerLunarDate
    {
        string LunarDay { get; }
        string LunarMonth { get; }
        int LunarYear { get; }
        string ZodiacYear { get; }
        string Stem { get; }
    }
    public class KhmerLunarDate : IKhmerLunarDate
    {
        public string LunarDay { get; set; } = string.Empty;
        public string LunarMonth { get; set; } = string.Empty;
        public int LunarYear { get; set; } = 0;
        public string ZodiacYear { get; set; } = string.Empty;
        public string Stem { get; set; } = string.Empty;
    }
    public static class KhmerCalendar
    {
        public static int GetKhmerLunarYear(this DateTime date)
        {
            int input = date.Year;
            if (date > new DateTime(input, 4, 14))
            {
                return input + 544;
            }
            return input + 543;
        }
        public static string GetKhmerZodiac(this DateTime date)
        {
            string[] khZodiac = [
              "ជូត",
              "ឆ្លូវ",
              "ខាល",
              "ថោះ",
              "រោង",
              "ម្សាញ់",
              "មមី",
              "មមែ",
              "វក",
              "រកា",
              "ច",
              "កុរ",
            ];
            return khZodiac[(date.GetKhmerLunarYear() - 2564) % 12];
        }
        public static string GetKhmerStem(this DateTime date)
        {
            string[] khStems = [
             "ឯកស័ក", "ទោស័ក", "ត្រីស័ក", "ចត្វាស័ក", "បញ្ចស័ក",
             "ឆស័ក", "សប្តស័ក", "អដ្ឋស័ក", "នព្វស័ក", "សំរឹទ្ធិស័ក",
            ];
            return khStems[(date.GetKhmerLunarYear() - 2563) % 10];
        }
        public static double ToJulianDay(this DateTime date)
        {
            double a = (14 - date.Month) / 12;
            double y = date.Year + 4800 - a;
            double m = date.Month + 12 * a - 3;
            double jd = date.Day + (153 * m + 2) / 5 + (365 * y) + (y / 4) - (y / 100) + (y / 400) - 32045;
            return jd;
        }
        public static IKhmerLunarDate ToKhmerLunarDate(this DateTime date)
        {
            string[] khMonths = [
                "ចេត្រ", "ពិសាខ", "ជេស្ឋ", "អាសាឍ", "ស្រាពណ៍", "ភទ្របទ",
                "អស្សុជ", "កត្តិក", "មិគសិរ", "បុស្ស", "មាឃ", "ផល្គុន"
            ];
            Dictionary<int, string> khDayString = new()
            {
                { 1,  "១កើត" },
                { 2,  "២កើត" },
                { 3,  "៣កើត" },
                { 4,  "៤កើត" },
                { 5,  "៥កើត" },
                { 6,  "៦កើត" },
                { 7,  "៧កើត" },
                { 8,  "៨កើត" },
                { 9,  "៩កើត" },
                { 10, "១០កើត" },
                { 11, "១១កើត" },
                { 12, "១២កើត" },
                { 13, "១៣កើត" },
                { 14, "១៤កើត" },
                { 15, "១៥កើត" },

                { 16, "១រោច" },
                { 17, "២រោច" },
                { 18, "៣រោច" },
                { 19, "៤រោច" },
                { 20, "៥រោច" },
                { 21, "៦រោច" },
                { 22, "៧រោច" },
                { 23, "៨រោច" },
                { 24, "៩រោច" },
                { 25, "១០រោច" },
                { 26, "១១រោច" },
                { 27, "១២រោច" },
                { 28, "១៣រោច" },
                { 29, "១៤រោច" },
                { 30, "១៥រោច" }
            };

            // Convert to Julian Day Number
            int julianDay = (int)Math.Floor(date.ToJulianDay());

            // Find the nearest new moon
            int k = (int)Math.Floor((julianDay - 2451545.0) / 29.530588853);
            int newMoonJd = GetNewMoonDay(k);

            if (newMoonJd > julianDay)
            {
                k -= 1;
                newMoonJd = GetNewMoonDay(k);
            }
            else if (GetNewMoonDay(k + 1) <= julianDay)
            {
                k += 1;
                newMoonJd = GetNewMoonDay(k);
            }
            int lunarDay = julianDay - newMoonJd + 1;
            if (lunarDay < 1)
            {
                k -= 1;
                newMoonJd = GetNewMoonDay(k);
                lunarDay = julianDay - newMoonJd + 1;
            }
            int refYear = date.Year;
            if (!(date.Month > 4 || (date.Month == 4 && date.Day >= 14)))
            {
                refYear = date.Year - 1;
            }
            double jdRef = new DateTime(refYear, 4, 14).ToJulianDay();
            int kRef = (int)Math.Floor((jdRef - 2451545.0) / 29.530588853);

            int monthCount = 0;
            int currentK = kRef;
            while (true)
            {
                int monthStart = GetNewMoonDay(currentK);
                int monthEnd = GetNewMoonDay(currentK + 1);

                if (monthStart > newMoonJd)
                    break;

                if (!IsLeapMonth(monthStart, monthEnd))
                {
                    monthCount++;
                }
                currentK++;
            }

            int lunarMonth = (monthCount) % 12;
            int lunarYear = GetKhmerLunarYear(date);

            string monthName = khMonths[lunarMonth];
            string zodiacYear = GetKhmerZodiac(date);
            string stem = GetKhmerStem(date);

            //return new()
            //{
            //    { "lunarDay", khDayString[lunarDay]},
            //    {"lunarMonth", monthName },
            //    {"lunarYear", $"{lunarYear}" },
            //    {"zodiacYear", zodiacYear },
            //    {"stem", stem },
            //};
            return new KhmerLunarDate
            {
                LunarDay = khDayString[lunarDay],
                LunarMonth = monthName,
                LunarYear = lunarYear,
                ZodiacYear = zodiacYear,
                Stem = stem,
            };
        }
        private static int GetNewMoonDay(int k, float timeZone = 7.0f)
        {
            double T = k / 1236.85;
            double JDE = (
                2451550.09766
                + 29.530588861 * k
                + 0.00015437 * Math.Pow(T, 2)
                - 0.000000150 * Math.Pow(T, 3)
                + 0.00000000073 * Math.Pow(T, 4)
            );
            JDE += timeZone / 24.0;
            return (int)Math.Floor(JDE + 0.5);
        }
        private static double GetSunLongitude(int jd)
        {
            double T = (jd - 2451545.0) / 36525.0;
            double meanLongitude = 280.46646 + 36000.76983 * T + 0.0003032 * Math.Pow(T, 2);
            double M = 357.52911 + 35999.05029 * T - 0.0001537 * Math.Pow(T, 2);
            M = M * Math.PI / 180.0;
            double C = (1.914602 - 0.004817 * T - 0.000014 * Math.Pow(T, 2)) * Math.Sin(M)
                        + (0.019993 - 0.000101 * T) * Math.Sin(2 * M)
                        + 0.000289 * Math.Sin(3 * M);
            double trueLong = (meanLongitude + C) % 360;
            return trueLong;
        }
        private static bool IsLeapMonth(int jdStart, int jdEnd)
        {
            double longStart = GetSunLongitude(jdStart);
            double longEnd = GetSunLongitude(jdEnd);
            for (int i = 1; i <= 12; i++)
            {
                int term = i * 30;
                if ((longEnd > term && term >= longStart) ||
                    (longEnd < longStart && (longStart <= term || term < longEnd)))
                {
                    return false;
                }
            }
            return true;
        }
    }
    internal static class Helper
    {
        internal static double CorrectDegreeRange(this double value)
        {
            value = value % 360;
            if (value < 0)
            {
                value += 360;
            }
            return value;
        }
    }
}
