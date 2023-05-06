using System;
using System.Globalization;
using System.Windows.Data;

namespace NaudioPlayer.Converters
{
    public class TimeSpanToTimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            }
            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
