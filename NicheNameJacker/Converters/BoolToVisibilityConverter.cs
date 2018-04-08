using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NicheNameJacker.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = Visibility.Collapsed;
            if (value is bool && (bool)value)
            {
                visibility = Visibility.Visible;
            }

            var invertResult = SafeParseBool(parameter);

            if (invertResult)
            {
                visibility = Opposite(visibility);
            }

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;
            bool returnValue = visibility == Visibility.Visible;
            bool invertResult;
            if (parameter != null && bool.TryParse(parameter.ToString(), out invertResult))
            {
                if (invertResult)
                {
                    returnValue = !(returnValue);
                }
            }
            return returnValue;
        }

        private static bool SafeParseBool(object parameter)
        {
            var parsed = false;
            if (parameter != null)
            {
                bool.TryParse(parameter.ToString(), out parsed);
            }
            return parsed;
        }

        private static Visibility Opposite(Visibility target)
        {
            if (target == Visibility.Visible)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
    }
}

