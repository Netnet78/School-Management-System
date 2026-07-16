using System.Globalization;
using System.Windows.Data;

namespace SchoolManagement.Presentation.Shared.XAMLConverters
{
    /// <summary>
    /// Converts null values to Boolean values and vice versa for data binding scenarios.
    /// </summary>
    /// <remarks>This converter is typically used in XAML data binding to represent null values as Boolean
    /// values in the user interface. The behavior can be customized using the FalseOnNull property.</remarks>
    public class NullToBooleanConverter : IValueConverter
    {
        public bool FalseOnNull { get; set; } = false;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            if (FalseOnNull)
            {
                return !isNull;
            }
            return isNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}