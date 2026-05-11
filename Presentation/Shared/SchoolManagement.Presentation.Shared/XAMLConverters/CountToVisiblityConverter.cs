using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SchoolManagement.Presentation.Shared.XAMLConverters
{
    public class CountToVisiblityConverter : IValueConverter
    {
        public bool IsReversed { get; set; } = false;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IEnumerable<object> e)
            {
                throw new Exception("Value must be enumerable");
            }

            int count = e.Count();

            if (!int.TryParse((string)parameter, out int param))
            {
                param = 0;
            }

            bool result = count > param;

            if (IsReversed) result = !result;

            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
