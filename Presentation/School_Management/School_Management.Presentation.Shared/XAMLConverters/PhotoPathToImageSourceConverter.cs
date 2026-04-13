using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace School_Management.Presentation.Shared.XAMLConverters
{
    public class PhotoPathToImageSourceConverter : IValueConverter
    {
        // If PhotoPath is a valid file path, load; otherwise return null (Image will be empty or use placeholder)
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path))
                return null;
            try
            {
                if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                    return new BitmapImage(new Uri(path, UriKind.Absolute));

                if (File.Exists(path))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(path, UriKind.Absolute);
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
            }
            catch
            {
                // swallow — return null to avoid breaking UI
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}