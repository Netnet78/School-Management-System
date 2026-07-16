using SchoolManagement.Core.Shared.Time;
using System.Globalization;
using System.Windows.Data;

namespace SchoolManagement.Presentation.Shared.XAMLConverters
{
    public class UTCToLocalTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dateTime)
            {
                return Binding.DoNothing;
            }

            return dateTime.ToLocalTimeZone();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dateTime)
            {
                return Binding.DoNothing;
            }

            return dateTime.ToUtcTimeZone();
        }
    }
}
