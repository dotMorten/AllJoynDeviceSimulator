using AdapterLib;
using BridgeRT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AllJoynSimulatorApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            CheckBridgeStatus();
        }

        private async void CheckBridgeStatus()
        {
            status.Text = "Starting up bridge...";
            try
            {
                await AllJoynDeviceManager.Current.StartupTask;
                status.Text = "Bridge Successfully Initialized";
            }
            catch(System.Exception ex)
            {
                status.Text = "Bridge failed to initialize:\n" + ex.Message;
                return;
            }

            var bulb = new MockLightingServiceHandler($"Mock Dimmable+Color+Temp Bulb", Guid.NewGuid().ToString(), true, true, true, this.Dispatcher);
            AllJoynDeviceManager.Current.AddBulb(bulb);
            bulb = new MockLightingServiceHandler($"Mock Dimmable+Temp Bulb", Guid.NewGuid().ToString(), true, false, true, this.Dispatcher);
            AllJoynDeviceManager.Current.AddBulb(bulb);
            bulb = new MockLightingServiceHandler($"Mock Dimmable Bulb", Guid.NewGuid().ToString(), true, false, false, this.Dispatcher);
            AllJoynDeviceManager.Current.AddBulb(bulb);
            bulb = new MockLightingServiceHandler($"Mock Bulb", Guid.NewGuid().ToString(), false, false, false, this.Dispatcher);
            AllJoynDeviceManager.Current.AddBulb(bulb);

            foreach (var b in AllJoynDeviceManager.Current.Bulbs)
            {
                b.LampState_Hue = 0;
                b.LampState_Brightness = UInt32.MaxValue;
                b.LampState_Saturation = b.LampDetails_Color ? UInt32.MaxValue : 0;
                b.LampState_OnOff = true;
            }
            this.DataContext = AllJoynDeviceManager.Current;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddBulbWindow.Visibility = Visibility.Visible;
            bulbName.Text = string.Format("Mock Bulb {0}", AllJoynDeviceManager.Current.Bulbs.Count() + 1);
        }

        private void Button_Click_OK(object sender, RoutedEventArgs e)
        {
            var bulb = new MockLightingServiceHandler(bulbName.Text, Guid.NewGuid().ToString(),
                switchDimming.IsOn, switchColor.IsOn, switchTemperature.IsOn, this.Dispatcher);
            bulb.LampState_Hue = 0;
            bulb.LampState_Brightness = UInt32.MaxValue;
            bulb.LampState_Saturation = bulb.LampDetails_Color ? UInt32.MaxValue : 0;
            bulb.LampState_OnOff = true;
            AllJoynDeviceManager.Current.AddBulb(bulb);
            Button_Click_Cancel(sender, e);
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            AddBulbWindow.Visibility = Visibility.Collapsed;
        }
    }
}
