using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kats
{
    internal class Base64ToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value == null) return null;

            string base64Image = (string)value;
            byte[] Base64Stream = System.Convert.FromBase64String(base64Image);
            return ImageSource.FromStream(() => new MemoryStream(Base64Stream));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
