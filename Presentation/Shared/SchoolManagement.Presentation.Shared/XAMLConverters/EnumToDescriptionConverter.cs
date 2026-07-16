using SchoolManagement.Core.Shared.Extensions;
using System.Windows.Data;

namespace SchoolManagement.Presentation.Shared.XAMLConverters
{
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null)
            {
                return string.Empty;
            }

            if (value is Enum enumValue)
            {
                return EnumExtensions.GetDescription(enumValue);
            }

            return value.ToString()!;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    }
}
