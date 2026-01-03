using System.Text;

namespace New_Student_Management.Helpers
{
    public static class EnglishToKhmer
    {
        public static string UseKhmerNumbers(this string input)
        {
            Dictionary<char, char> numbers = new()
            {
                {'0', '០'},
                { '1', '១' },
                { '2', '២' },
                { '3', '៣' },
                { '4', '៤' },
                { '5', '៥' },
                { '6', '៦' },
                { '7', '៧' },
                { '8', '៨' },
                { '9', '៩' },
            };
            StringBuilder sb = new();
            foreach (char c in input)
            {
                if (numbers.TryGetValue(c, out char value))
                {
                    sb.Append(value);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public static string UseKhmerNumbers(this int input)
        {
            Dictionary<char, char> numbers = new()
            {
                {'0', '០'},
                { '1', '១' },
                { '2', '២' },
                { '3', '៣' },
                { '4', '៤' },
                { '5', '៥' },
                { '6', '៦' },
                { '7', '៧' },
                { '8', '៨' },
                { '9', '៩' },
            };
            StringBuilder sb = new();
            foreach (char n in input.ToString())
            {
                if (numbers.TryGetValue(n, out char value))
                {
                    sb.Append(value);
                }
            }
            return sb.ToString();
        }
        public static string UseKhmerDays(this string input)
        {
            Dictionary<string, string> days = new()
            {
                { "Monday", "ចន្ទ" },
                { "Tuesday", "អង្គារ" },
                { "Wednesday", "ពុធ" },
                { "Thursday", "ព្រហស្បតិ៍" },
                { "Friday", "សុក្រ" },
                { "Saturday", "សៅរ៍" },
                { "Sunday", "អាទិត្យ" },
            };
            foreach (var day in days)
            {
                input = input.Replace(day.Key, day.Value);
            }
            return input;
        }
        public static string UseKhmerDays(this int input)
        {
            Dictionary<int, string> days = new()
            {
                { 1, "ចន្ទ" },
                { 2, "អង្គារ" },
                { 3, "ពុធ" },
                { 4, "ព្រហស្បតិ៍" },
                { 5, "សុក្រ" },
                { 6, "សៅរ៍" },
                { 0, "អាទិត្យ" },
            };
            if (days.TryGetValue(input, out string? data))
            {
                return data;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Input must be between 0 and 6.");
            }
        }
        public static string UseKhmerMonths(this string input)
        {
            Dictionary<string, string> months = new()
            {
                { "January", "មករា" },
                { "February", "កុម្ភៈ" },
                { "March", "មីនា" },
                { "April", "មេសា" },
                { "May", "ឧសភា" },
                { "June", "មិថុនា" },
                { "July", "កក្កដា" },
                { "August", "សីហា" },
                { "September", "កញ្ញា" },
                { "October", "តុលា" },
                { "November", "វិច្ឆិកា" },
                { "December", "ធ្នូ" },
            };
            foreach (var month in months)
            {
                input = input.Replace(month.Key, month.Value);
            }
            return input;
        }
        public static string UseKhmerMonths(this int input)
        {
            Dictionary<int, string> months = new()
            {
                { 1, "មករា" },
                { 2, "កុម្ភៈ" },
                { 3, "មីនា" },
                { 4, "មេសា" },
                { 5, "ឧសភា" },
                { 6, "មិថុនា" },
                { 7, "កក្កដា" },
                { 8, "សីហា" },
                { 9, "កញ្ញា" },
                { 10, "តុលា" },
                { 11, "វិច្ឆិកា" },
                { 12, "ធ្នូ" },
            };
            if (months.TryGetValue(input, out string? data))
            {
                return data;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Input must be between 1 and 12.");
            }
        }
    }
}
