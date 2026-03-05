using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FancyCards.Converters
{
    internal class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timespan = (TimeSpan)value;

            
            if(parameter != null && parameter.ToString().ToLower() == "minutes")
            {
                return string.Format("{0:00}:{1:00}", (int)timespan.TotalMinutes, timespan.Seconds);
            }
            return string.Format("{0:00}:{1:00}:{2:00}", (int)timespan.TotalHours, timespan.Minutes, timespan.Seconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
