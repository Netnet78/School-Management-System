using System.Globalization;
using System.Windows.Data;

namespace SchoolManagement.Presentation.Shared.XAMLConverters
{
    public class BooleanInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool b)
            {
                return false;
            }
            return !b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b!;
            }
            return false;
        }
    }
}
