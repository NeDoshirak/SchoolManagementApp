using System;
using System.Globalization;
using System.Windows.Data;

namespace Presentation.Converters
{
    public class QuarterDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is int quarterNumber && values[1] is string academicYear)
            {
                return $"{quarterNumber} четверть ({academicYear})";
            }
            return "Неизвестная четверть";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}