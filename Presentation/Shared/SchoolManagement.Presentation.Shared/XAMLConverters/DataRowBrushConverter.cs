using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SchoolManagement.Presentation.Shared.XAMLConverters
{
    // MultiValueConverter that inspects Student properties and returns a background Brush
    // Values expected (order): PhotoPath, FirstName, LastName, Gender, Skill,
    // BirthVillage, BirthCommune, BirthDistrict, BirthProvince,
    // FatherName, MotherName, FatherOccupation, MotherOccupation, Religion,
    // ExamCenter, ExamDate, ExamTable, ExamRoom, FromSchool
    public sealed class DataRowBrushConverter : IMultiValueConverter
    {
        private static readonly Brush TransparentBrush = Brushes.Transparent;
        private static readonly Brush WarningBrush = Brushes.Yellow;
        private static readonly Brush CriticalBrush = Brushes.OrangeRed;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return TransparentBrush;

            var rawPhoto = values[0];

            if (rawPhoto == null || rawPhoto == DependencyProperty.UnsetValue)
                return CriticalBrush;

            var photoPath = rawPhoto.ToString();

            if (string.IsNullOrWhiteSpace(photoPath))
                return CriticalBrush;

            // Check other fields
            foreach (var v in values.Skip(1))
            {
                if (v == null || v == DependencyProperty.UnsetValue)
                    return WarningBrush;

                var s = v.ToString();
                if (string.IsNullOrWhiteSpace(s))
                    return WarningBrush;
            }

            return TransparentBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}