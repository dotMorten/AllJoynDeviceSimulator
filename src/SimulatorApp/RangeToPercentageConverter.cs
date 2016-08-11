using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AllJoynSimulatorApp
{
    public class RangeToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double parameterVal;
            if(value != null && parameter != null &&
                double.TryParse(parameter as string, out parameterVal))
            {
                var percentage = (double)System.Convert.ChangeType(value, typeof(double)) / parameterVal * 100;
                return string.Format($"{percentage:0.0}%");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class HueToDegreesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var percentage = (double)System.Convert.ChangeType(value, typeof(double)) / 4294967294 * 360;
                return string.Format($"{percentage:0.0}°");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class UIntToKelvinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var v = (double)System.Convert.ChangeType(value, typeof(double));
                var kelvin = v / 4294967294 * 19000 + 1000;
                return string.Format($"{kelvin:0}°K");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
