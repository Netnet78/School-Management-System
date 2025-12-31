using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Student_Management.Converters
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
            if (value is bool b)
            {
                bool data = IsReversed ? !b : b;
                Visibility visibility = data ? (Visibility.Visible): (Collapse ? Visibility.Collapsed : Visibility.Hidden);
                return visibility;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
