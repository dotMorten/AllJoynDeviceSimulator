using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AllJoynSimulatorApp.DeviceViews
{
    public class DeviceView : ContentControl
    {
        public object Device
        {
            get { return (object)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(object), typeof(DeviceView), new PropertyMetadata(null, OnDevicePropertyChanged));

        private static void OnDevicePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DeviceView)d).UpdateView(e.NewValue);
        }

        private void UpdateView(object newValue)
        {
            if(newValue == null)
            {
                Content = null;
            }
            else if(newValue is AdapterLib.MockBulbDevice)
            {
                Content = new LightBulbView() { DataContext = newValue };
            }
            else if(newValue is AdapterLib.MockCurrentTemperatureDevice)
            {
                Content = new TemperatureView() { Device = newValue as AdapterLib.MockCurrentTemperatureDevice };
            }
            else if (newValue is AdapterLib.MockCurrentHumidityDevice)
            {
                Content = new HumidityView() { Device = newValue as AdapterLib.MockCurrentHumidityDevice };
            }
            else if (newValue is AdapterLib.MockOnOffSwitchDevice)
            {
                Content = new SwitchView() { Device = newValue as AdapterLib.MockOnOffSwitchDevice };
            }
        }
    }
}
