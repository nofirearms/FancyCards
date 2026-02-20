
using System.Globalization;
using System.Windows.Data;

namespace FancyCards.Converters
{
    internal class DateToDaysPassedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime?)value;

            if (date is null || date == default) return "never";

            var days_ago = Math.Floor((DateTime.Now - (DateTime)date).TotalDays);

            switch (days_ago)
            {
                case 0:
                    return "today";
                case 1:
                    return "yesterday";
                default:
                    return $"{days_ago} days ago";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
