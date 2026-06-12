using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SchoolManagement.Presentation.Shared.Converters;

namespace SchoolManagement.Presentation.Features.Reports.Converters;

public class BytesToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte[] bytes && bytes.Length > 0)
            return bytes.ConvertToBitmapsource();

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
