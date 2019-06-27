using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace EarthquakeTalkerClient
{
    class MessageTextVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            bool rev = (bool)parameter;

            if (string.IsNullOrEmpty(text)
                || Util.CheckImageUri(text))
            {
                return rev ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return rev ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
