using AdapterLib;
using AllJoyn.Dsb;
using BridgeRT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
                SaveDevices();
                var d = e.SuspendingOperation.GetDeferral();
                await AllJoynDsbServiceManager.Current.ShutdownAsync();
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
                var config = new BridgeConfiguration(GetDeviceID(), "com.dotMorten.TestApp")
                {
                    // The following are optional. If not set will be pulled from the package information and system information
                    ModelName = "AllJoyn Mock DSB",
                    DeviceName = "AllJoyn Simulator",
                    ApplicationName = "AllJoyn Device Simulator",
                    Vendor = "MockDevices Inc"
                };
                await AllJoynDsbServiceManager.Current.StartAsync(config);
                status.Text = ""; // Bridge Successfully Initialized
                addDeviceBox.IsEnabled = true;
            }
            catch (System.Exception ex)
            {
                status.Text = "Bridge failed to initialize:\n" + ex.Message;
                return;
            }
            LoadDevices();
        }

        private Guid GetDeviceID()
        {
            if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("DSBDeviceId"))
            {
                Guid deviceId = Guid.NewGuid();
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["DSBDeviceId"] = deviceId;
                return deviceId;
            }
            return (Guid)Windows.Storage.ApplicationData.Current.LocalSettings.Values["DSBDeviceId"];
        }
        private void LoadDevices()
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (!settings.Containers.ContainsKey("Devices"))
            {
                // Create a set of initial bulbs
                var bulb = new MockLightingServiceHandler($"Mock Advanced Bulb", Guid.NewGuid().ToString(), true, true, true, this.Dispatcher);
                AllJoynDsbServiceManager.Current.AddDevice(new MockBulbDevice(bulb));
                bulb = new MockLightingServiceHandler($"Mock Simple Bulb", Guid.NewGuid().ToString(), true, false, false, this.Dispatcher);
                AllJoynDsbServiceManager.Current.AddDevice(new MockBulbDevice(bulb));
                AllJoynDsbServiceManager.Current.AddDevice(new MockCurrentHumidityDevice("Mock Humidity Sensor", Guid.NewGuid().ToString(), 50));
                AllJoynDsbServiceManager.Current.AddDevice(new MockCurrentTemperatureDevice("Mock Temperature Sensor", Guid.NewGuid().ToString(), 25));
                AllJoynDsbServiceManager.Current.AddDevice(new MockOnOffSwitchDevice("Mock Switch", Guid.NewGuid().ToString(), false, Dispatcher));
            }
            else
            {
                var container = settings.Containers["Devices"];
                foreach(var item in container.Values)
                {
                    var data = ((string)item.Value).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var type = data[0].Trim();
                    if (type == "Lamp")
                    {
                        var bulb = MockLightingServiceHandler.FromJson(data[1], Dispatcher);
                        AllJoynDsbServiceManager.Current.AddDevice(new MockBulbDevice(bulb));
                    }
                    else if(type == "CurrentTemperature")
                    {
                        var d = new MockCurrentTemperatureDevice(
                            data[2], data[1], double.Parse(data[3], CultureInfo.InvariantCulture));
                        AllJoynDsbServiceManager.Current.AddDevice(d);
                    }
                    else if (type == "CurrentHumidity")
                    {
                        var d = new MockCurrentHumidityDevice(
                            data[2], data[1], double.Parse(data[3], CultureInfo.InvariantCulture));
                        AllJoynDsbServiceManager.Current.AddDevice(d);
                    }
                    else if (type == "OnOffSwitch")
                    {
                        var d = new MockOnOffSwitchDevice(data[2], data[1], data[3] == "true", Dispatcher);
                        AllJoynDsbServiceManager.Current.AddDevice(d);
                    }
                }
            }

            this.DataContext = AllJoynDsbServiceManager.Current;
        }
        
        private void SaveDevices()
        {
            var container = ApplicationData.Current.LocalSettings.CreateContainer("Devices", ApplicationDataCreateDisposition.Always);
            container.Values.Clear();
            int i = 0;
            foreach (var b in AllJoynDsbServiceManager.Current.Devices)
            {
                StringBuilder sb = new StringBuilder();
                if(b is MockBulbDevice)
                {
                    sb.AppendLine("Lamp");
                    sb.Append((((MockBulbDevice)b).LightingServiceHandler as MockLightingServiceHandler).ToJson());
                }
                else if(b is MockCurrentTemperatureDevice)
                {
                    sb.AppendLine("CurrentTemperature");
                    sb.AppendLine(((MockCurrentTemperatureDevice)b).SerialNumber);
                    sb.AppendLine(((MockCurrentTemperatureDevice)b).Name);
                    sb.AppendLine(((MockCurrentTemperatureDevice)b).CurrentValue.ToString(CultureInfo.InvariantCulture));
                }
                else if(b is MockCurrentHumidityDevice)
                {
                    sb.AppendLine("CurrentHumidity");
                    sb.AppendLine(((MockCurrentHumidityDevice)b).SerialNumber);
                    sb.AppendLine(((MockCurrentHumidityDevice)b).Name);
                    sb.AppendLine(((MockCurrentHumidityDevice)b).CurrentValue.ToString(CultureInfo.InvariantCulture));
                }
                else if (b is MockOnOffSwitchDevice)
                {
                    sb.AppendLine("OnOffSwitch");
                    sb.AppendLine(((MockOnOffSwitchDevice)b).SerialNumber);
                    sb.AppendLine(((MockOnOffSwitchDevice)b).Name);
                    sb.AppendLine(((MockOnOffSwitchDevice)b).OnOff ? "true" : "false");
                }
                else
                {
                    continue;
                }
                container.Values[i++.ToString("0000")] = sb.ToString();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addDeviceBox.SelectedIndex == 0) return;
            var idx = addDeviceBox.SelectedIndex;
            addDeviceBox.SelectedIndex = 0;
            if (idx == 1)
            {
                AddBulbWindow.Visibility = Visibility.Visible;
                bulbName.Text = string.Format("Mock Bulb {0}", AllJoynDsbServiceManager.Current.Devices.OfType<MockBulbDevice>().Count() + 1);
            }
            else if(idx == 2)
            {
                AddSensorWindow.Visibility = Visibility.Visible;
                sensorName.Text = string.Format("Temperature Sensor {0}", AllJoynDsbServiceManager.Current.Devices.OfType<MockCurrentTemperatureDevice>().Count() + 1);
                sensorName.Tag = "Temperature";
            }
            else if (idx == 3)
            {
                AddSensorWindow.Visibility = Visibility.Visible;
                sensorName.Text = string.Format("Humidity Sensor {0}", AllJoynDsbServiceManager.Current.Devices.OfType<MockCurrentHumidityDevice>().Count() + 1);
                sensorName.Tag = "Humidity";
            }
            else if (idx == 4)
            {
                AddSensorWindow.Visibility = Visibility.Visible;
                sensorName.Text = string.Format("Switch {0}", AllJoynDsbServiceManager.Current.Devices.OfType<MockOnOffSwitchDevice>().Count() + 1);
                sensorName.Tag = "Switch";
            }
        }

        private void Button_Click_OK(object sender, RoutedEventArgs e)
        {
            var bulb = new MockLightingServiceHandler(bulbName.Text, Guid.NewGuid().ToString(),
                switchDimming.IsOn, switchColor.IsOn, switchTemperature.IsOn, this.Dispatcher);
            bulb.LampState_Hue = (UInt32)(new Random().NextDouble() * (uint.MaxValue - 1));
            bulb.LampState_Brightness = UInt32.MaxValue - 1;            
            bulb.LampState_Saturation = UInt32.MaxValue - 1;
            bulb.LampState_OnOff = true;
            AllJoynDsbServiceManager.Current.AddDevice(new MockBulbDevice(bulb));
            SaveDevices();
            Button_Click_Cancel(sender, e);
        }

        private void AddSensorButton_Click_OK(object sender, RoutedEventArgs e)
        {
            if(sensorName.Tag as string == "Temperature")
            {
                AllJoynDsbServiceManager.Current.AddDevice(new MockCurrentTemperatureDevice(sensorName.Text, Guid.NewGuid().ToString(), 25));
                SaveDevices();
            }
            else if(sensorName.Tag as string == "Humidity")
            {
                AllJoynDsbServiceManager.Current.AddDevice(new MockCurrentHumidityDevice(sensorName.Text, Guid.NewGuid().ToString(), 50));
                SaveDevices();
            }
            else if (sensorName.Tag as string == "Switch")
            {
                AllJoynDsbServiceManager.Current.AddDevice(new MockOnOffSwitchDevice(sensorName.Text, Guid.NewGuid().ToString(), false, Dispatcher));
                SaveDevices();
            }
            Button_Click_Cancel(sender, e);
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            AddBulbWindow.Visibility = AddSensorWindow.Visibility = Visibility.Collapsed;
        }

        private void Delete_Item_Tapped(object sender, RoutedEventArgs e)
        {
            var device = (sender as FrameworkElement).DataContext as IAdapterDevice;
            if(device != null)
            {
                AllJoynDsbServiceManager.Current.RemoveDevice(device);
                SaveDevices();
            }
        }

        private void Button_Click_Help(object sender, RoutedEventArgs e)
        {
            var _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/dotMorten/AllJoynDeviceSimulator/wiki/Help"));
        }
    }
}
