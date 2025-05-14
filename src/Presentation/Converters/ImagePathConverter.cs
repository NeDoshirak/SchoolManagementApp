using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Presentation.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string path))
                return null;

            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            if (File.Exists(fullPath))
            {
                return new BitmapImage(new Uri(fullPath));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}