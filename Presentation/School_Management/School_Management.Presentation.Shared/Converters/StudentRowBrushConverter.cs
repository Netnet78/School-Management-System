using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace School_Management.Presentation.Shared.Converters
{
    // MultiValueConverter that inspects Student properties and returns a background Brush
    // Values expected (order): PhotoPath, FirstName, LastName, Gender, Skill,
    // BirthVillage, BirthCommune, BirthDistrict, BirthProvince,
    // FatherName, MotherName, FatherOccupation, MotherOccupation, Religion,
    // ExamCenter, ExamDate, ExamTable, ExamRoom, FromSchool
    public sealed class StudentRowBrushConverter : IMultiValueConverter
    {
        private static readonly Brush TransparentBrush = Brushes.Transparent;
        private static readonly Brush WarningBrush = Brushes.Yellow;
        private static readonly Brush CriticalBrush = Brushes.OrangeRed;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return TransparentBrush;

            // PhotoPath is the first value
            var photoPath = values[0] as string;
            if (string.IsNullOrWhiteSpace(photoPath))
            {
                return CriticalBrush;
            }

            // any other required string fields empty? (skip index 0)
            foreach (var v in values.Skip(1))
            {
                string s = v.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(s))
                    return WarningBrush;
            }

            return TransparentBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}