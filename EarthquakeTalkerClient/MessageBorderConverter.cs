using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace EarthquakeTalkerClient
{
    class MessageBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = (Message.Priority)value;

            switch (level)
            {
                case Message.Priority.Low:
                case Message.Priority.Normal:
                    return Brushes.Gainsboro;

                case Message.Priority.High:
                case Message.Priority.Critical:
                    return Brushes.Red;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
