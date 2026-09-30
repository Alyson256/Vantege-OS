using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VantageOS.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If parameter is "Invert", invert the logic
                bool invert = parameter?.ToString() == "Invert";
                return (boolValue != invert) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }

    public class PercentToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                if (percent >= 90) return Application.Current.FindResource("Rose500Brush");
                if (percent >= 70) return Application.Current.FindResource("Amber500Brush");
                return Application.Current.FindResource("Emerald500Brush");
            }
            return Application.Current.FindResource("Zinc400Brush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToPlaceholderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string placeholder = parameter?.ToString() ?? "—";
            if (value == null) return placeholder;
            if (value is string s && string.IsNullOrWhiteSpace(s)) return placeholder;
            if (value is double d && d == 0) return placeholder;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
