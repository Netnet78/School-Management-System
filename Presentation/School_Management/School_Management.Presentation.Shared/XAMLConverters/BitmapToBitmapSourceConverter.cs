using School_Management.Presentation.Shared.Converters;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace School_Management.Presentation.Shared.XAMLConverters
{
    public class BitmapToBitmapSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Bitmap image)
            {
                return DependencyProperty.UnsetValue;
            }

            return image.ConvertToBitmapsource();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
