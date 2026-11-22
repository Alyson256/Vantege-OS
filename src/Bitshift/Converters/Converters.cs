using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Bitshift.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter?.ToString() == "Invert";

            if (value is bool boolValue)
            {
                return (boolValue != invert) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is string strValue)
            {
                bool hasText = !string.IsNullOrWhiteSpace(strValue);
                return (hasText != invert) ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}
