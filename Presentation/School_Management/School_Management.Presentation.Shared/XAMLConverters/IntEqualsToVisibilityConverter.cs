using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace School_Management.Presentation.Shared.XAMLConverters
{
    /// <summary>
    /// Converts an integer value to a Visibility value based on whether it equals a specified parameter. Intended for
    /// use in data binding scenarios where UI visibility depends on an integer comparison.
    /// </summary>
    /// <remarks>This converter is typically used in XAML bindings to show or hide UI elements depending on
    /// whether a bound integer value matches a given parameter. The comparison is performed using integer equality. If
    /// either the value or the parameter cannot be parsed as an integer, or if either is null, the converter returns
    /// Visibility.Collapsed.</remarks>
    public class IntEqualsToVisibilityConverter : IValueConverter
    {
        // parameter expected as integer (or string parseable to int)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            if (!int.TryParse(value.ToString(), out int v)) return Visibility.Collapsed;
            if (parameter == null) return Visibility.Collapsed;
            if (!int.TryParse(parameter.ToString(), out int param)) return Visibility.Collapsed;
            return v == param ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}