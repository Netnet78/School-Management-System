using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace Student_Management.Converters
{
    /// <summary>
    /// Provides a value converter that returns a visibility value based on whether a specified file path exists.
    /// </summary>
    /// <remarks>This converter is typically used in data binding scenarios to control the visibility of UI
    /// elements depending on the existence of a file at the given path. It implements the IValueConverter interface for
    /// use in WPF applications.</remarks>
    public class ValidPathToVisibility : IValueConverter
    {
        public bool Reversed { get; set; } = false;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrWhiteSpace(path))
            {
                bool file = System.IO.File.Exists(path);
                object result = file ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                return !Reversed ? result : !file ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            // If the value is not a valid string path, return Collapsed or Visible based on Reversed
            return !Reversed ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
