using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace FancyCards.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (object.Equals(parameter, "Reversed"))
            {
                return (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
            }
                
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
