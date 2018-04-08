using System;
using System.Globalization;
using System.Windows.Data;

namespace NicheNameJacker.Converters
{
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Invert(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Invert(value);

        bool? Invert(object value)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return null;
        }
    }
}

