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
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AllJoynSimulatorApp
{
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            CheckBridgeStatus();
            App.Current.Suspending += Current_Suspending;
            App.Current.Resuming += Current_Resuming;
        }

        private async void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            if (this.DataContext != null)
            {
                this.DataContext = null;
                SaveBulbs();
                var d = e.SuspendingOperation.GetDeferral();
                await AllJoynDeviceManager.Current.Shutdown();
                d.Complete();
            }
        }

        private void Current_Resuming(object sender, object e)
        {
            CheckBridgeStatus();
        }

        private async void CheckBridgeStatus()
        {
            status.Text = "Starting up bridge...";
            try
            {
                await AllJoynDeviceManager.Current.StartupTask;
                status.Text = ""; // Bridge Successfully Initialized
                addBulbButton.IsEnabled = true;
            }
            catch (System.Exception ex)
            {
                status.Text = "Bridge failed to initialize:\n" + ex.Message;
                return;
            }
            AllJoynDeviceManager.Current.NotificationRecieved += Current_NotificationRecieved;
            LoadBulbs();
        }

        private void Current_NotificationRecieved(object sender, AllJoynDeviceManager.NotificationEventArgs e)
        {
            //TODO
            //System.Diagnostics.Debugger.Break();
        }

        private void LoadBulbs()
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (!settings.Containers.ContainsKey("Bulbs"))
            {
                // Create a set of initial bulbs
                var bulb = new MockLightingServiceHandler($"Mock Dimmable+Color+Temp Bulb", Guid.NewGuid().ToString(), true, true, true, this.Dispatcher);
                AllJoynDeviceManager.Current.AddBulb(bulb);
                bulb = new MockLightingServiceHandler($"Mock Dimmable+Temp Bulb", Guid.NewGuid().ToString(), true, false, true, this.Dispatcher);
                AllJoynDeviceManager.Current.AddBulb(bulb);
                bulb = new MockLightingServiceHandler($"Mock Dimmable Bulb", Guid.NewGuid().ToString(), true, false, false, this.Dispatcher);
                AllJoynDeviceManager.Current.AddBulb(bulb);
                bulb = new MockLightingServiceHandler($"Mock Bulb", Guid.NewGuid().ToString(), false, false, false, this.Dispatcher);
                AllJoynDeviceManager.Current.AddBulb(bulb);
            }
            else
            {
                var container = settings.Containers["Bulbs"];
                foreach(var item in container.Values)
                {
                    var bulb = MockLightingServiceHandler.FromJson((string)item.Value, Dispatcher);
                    AllJoynDeviceManager.Current.AddBulb(bulb);
                }
            }

            this.DataContext = AllJoynDeviceManager.Current;
        }

        private void SaveBulbs()
        {
            var container = ApplicationData.Current.LocalSettings.CreateContainer("Bulbs", ApplicationDataCreateDisposition.Always);
            container.Values.Clear();
            int i = 0;
            foreach(var b in AllJoynDeviceManager.Current.Bulbs)
            {
                container.Values[i++.ToString("0000")] = b.ToJson();
            }
        }

        private void Button_Click_AddBulb(object sender, RoutedEventArgs e)
        {
            AddBulbWindow.Visibility = Visibility.Visible;
            bulbName.Text = string.Format("Mock Bulb {0}", AllJoynDeviceManager.Current.Bulbs.Count() + 1);
        }

        private void Button_Click_OK(object sender, RoutedEventArgs e)
        {
            var bulb = new MockLightingServiceHandler(bulbName.Text, Guid.NewGuid().ToString(),
                switchDimming.IsOn, switchColor.IsOn, switchTemperature.IsOn, this.Dispatcher);
            bulb.LampState_Hue = (UInt32)(new Random().NextDouble() * (uint.MaxValue - 1));
            bulb.LampState_Brightness = UInt32.MaxValue - 1;            
            bulb.LampState_Saturation = UInt32.MaxValue - 1;
            bulb.LampState_OnOff = true;
            AllJoynDeviceManager.Current.AddBulb(bulb);
            SaveBulbs();
            Button_Click_Cancel(sender, e);
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            AddBulbWindow.Visibility = Visibility.Collapsed;
        }

        private void Delete_Item_Tapped(object sender, RoutedEventArgs e)
        {
            var bulb = (sender as FrameworkElement).DataContext as MockLightingServiceHandler;
            AllJoynDeviceManager.Current.RemoveBulb(bulb);
            SaveBulbs();
        }

        private void Button_Click_Help(object sender, RoutedEventArgs e)
        {
            var _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/dotMorten/AllJoynDeviceSimulator/wiki/Help"));
        }
    }
}
