using KhmerCalendar;

namespace SchoolManagement.Application.Features.Reports.Helpers
{
    public static class ReportDateHelper
    {
        public static string FormatAcademicYear(int startYear, int endYear)
        {
            return $"ឆ្នាំសិក្សា {startYear}-{endYear}".UseKhmerNumbers();
        }

        public static string FormatDateOfBirth(DateOnly dob)
        {
            string month = dob.Month.UseKhmerMonths();
            return $"{dob.Day} {month} {dob.Year}".UseKhmerNumbers();
        }

        public static string FormatKhmerLunarDate(DateTime date)
        {
            string weekDay = ((int)date.DayOfWeek).UseKhmerDays();
            IKhmerLunarDate khmerLunar = date.ToKhmerLunarDate();
            string day = khmerLunar.LunarDay;
            string month = khmerLunar.LunarMonth;
            int year = khmerLunar.LunarYear;
            string zodiac = khmerLunar.ZodiacYear;
            string stem = khmerLunar.Stem;

            return $"ថ្ងៃ{weekDay}  ទី{day} ខែ{month} ឆ្នាំ{zodiac} {stem} ព.ស {year}".UseKhmerNumbers();
        }

        public static string FormatGregorianDate(DateOnly date)
        {
            string day = date.Day.UseKhmerNumbers();
            string month = date.Month.UseKhmerMonths();
            string year = date.Year.UseKhmerNumbers();

            return $"ថ្ងៃទី {day} ខែ {month} ឆ្នាំ {year}";
        }

        public static string FormatGregorianDateLine(string location, DateTime date)
        {
            string normalizedLocation = location.Trim();

            string day = date.Day.UseKhmerNumbers();
            string month = date.Month.UseKhmerMonths();
            string year = date.Year.UseKhmerNumbers();

            return $"{normalizedLocation} ថ្ងៃទី{day} ខែ{month} ឆ្នាំ{year}";
        }
    }
}
