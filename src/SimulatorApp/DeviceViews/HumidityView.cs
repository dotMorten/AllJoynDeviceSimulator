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
    public sealed class HumidityView : Control
    {
        public HumidityView()
        {
            this.DefaultStyleKey = typeof(HumidityView);
        }



        public AdapterLib.MockCurrentHumidityDevice Device
        {
            get { return (AdapterLib.MockCurrentHumidityDevice)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(AdapterLib.MockCurrentHumidityDevice), typeof(HumidityView), new PropertyMetadata(null));
    }
}
