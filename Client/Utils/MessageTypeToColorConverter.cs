
using System;
using System.Globalization;
using System.Windows.Data;

namespace Client.Utils
{
    [ValueConversion(typeof(string), typeof(string))]
    public class MessageTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var result = "#2a3345";
            if ((string) value == "Error")
            {
                result = "#ff0000";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}