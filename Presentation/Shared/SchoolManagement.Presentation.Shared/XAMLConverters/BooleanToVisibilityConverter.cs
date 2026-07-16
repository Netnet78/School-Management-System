using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SchoolManagement.Presentation.Shared.XAMLConverters
{
    /// <summary>
    /// 
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsReversed { get; set; } = false;
        public bool Collapse { get; set; } = false;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b;
            if (value is bool boolVal)
                b = boolVal;
            else
                b = value is not null;

            bool data = IsReversed ? !b : b;
            return data ? Visibility.Visible : (Collapse ? Visibility.Collapsed : Visibility.Hidden);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
