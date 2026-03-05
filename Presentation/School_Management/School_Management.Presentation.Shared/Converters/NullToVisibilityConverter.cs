using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace School_Management.Presentation.Shared.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public bool CollapseWhenNull { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = (value == null) || (value is string s && string.IsNullOrWhiteSpace(s));

            if (parameter is string p && p == "IsReversed") isNull = !isNull;

            return isNull ? (CollapseWhenNull ? Visibility.Collapsed : Visibility.Hidden) : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}