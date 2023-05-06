using System;
using System.Globalization;
using System.Windows.Data;

namespace NaudioPlayer.Converters
{
    public class DayOfWeekToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DayOfWeek dayOfWeek)
            {
                return dayOfWeek.ToString();
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
