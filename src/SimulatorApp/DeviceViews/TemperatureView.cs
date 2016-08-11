using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace AllJoynSimulatorApp.DeviceViews
{
    public sealed class TemperatureView : Control
    {
        public TemperatureView()
        {
            this.DefaultStyleKey = typeof(TemperatureView);
        }



        public AdapterLib.MockCurrentTemperatureDevice Device
        {
            get { return (AdapterLib.MockCurrentTemperatureDevice)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(AdapterLib.MockCurrentTemperatureDevice), typeof(TemperatureView), new PropertyMetadata(null));
    }
}
