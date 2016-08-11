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
    public sealed class SwitchView : Control
    {
        public SwitchView()
        {
            this.DefaultStyleKey = typeof(SwitchView);
        }
        
        public AdapterLib.MockOnOffSwitchDevice Device
        {
            get { return (AdapterLib.MockOnOffSwitchDevice)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(AdapterLib.MockOnOffSwitchDevice), typeof(SwitchView), new PropertyMetadata(null));
    }
}
