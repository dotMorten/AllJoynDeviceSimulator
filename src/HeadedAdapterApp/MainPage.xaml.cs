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
        public DsbBridge dsbBridge
        {
            get; private set;
        }

        public Task startupTask
        {
            get; private set;
        }

        public MainPage()
        {
            this.InitializeComponent();
            Initialize();
            CheckBridgeStatus();
        }
        Adapter adapter;
        public void Initialize()
        {

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            startupTask = ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction action) =>
            {
                try
                {
                    adapter = new Adapter();
                    dsbBridge = new DsbBridge(adapter);

                    var initResult = dsbBridge.Initialize();
                    if (initResult != 0)
                    {
                        throw new Exception("DSB Bridge initialization failed!");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            })).AsTask();

        }
        private async void CheckBridgeStatus()
        {
            status.Text = "Starting up bridge...";
            try
            {
                await startupTask;
                status.Text = "Bridge Successfully Initialized";
            }
            catch(System.Exception ex)
            {
                status.Text = "Bridge failed to initialize:\n" + ex.Message;
            }

            var bulb = new MockLightingServiceHandler($"Mock Dimmable+Color+Temp Bulb", Guid.NewGuid().ToString(), true, true, true, this.Dispatcher);
            Bulbs.Add(bulb);
            bulb = new MockLightingServiceHandler($"Mock Dimmable+Temp Bulb", Guid.NewGuid().ToString(), true, false, true, this.Dispatcher);
            Bulbs.Add(bulb);
            bulb = new MockLightingServiceHandler($"Mock Dimmable Bulb", Guid.NewGuid().ToString(), true, false, false, this.Dispatcher);
            Bulbs.Add(bulb);
            bulb = new MockLightingServiceHandler($"Mock Bulb", Guid.NewGuid().ToString(), false, false, false, this.Dispatcher);
            Bulbs.Add(bulb);

            foreach (var b in Bulbs)
            {
                b.LampState_Hue = 0;
                b.LampState_Brightness = UInt32.MaxValue;
                b.LampState_Saturation = b.LampDetails_Color ? UInt32.MaxValue : 0;
                b.LampState_OnOff = true;
                adapter.AddBulb(b);
            }
            this.DataContext = Bulbs;
        }

        public ObservableCollection<MockLightingServiceHandler> Bulbs { get; set; } = new ObservableCollection<MockLightingServiceHandler>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddBulbWindow.Visibility = Visibility.Visible;
            bulbName.Text = string.Format("Mock Bulb {0}", Bulbs.Count + 1);
        }

        private void Button_Click_OK(object sender, RoutedEventArgs e)
        {
            var bulb = new MockLightingServiceHandler(bulbName.Text, Guid.NewGuid().ToString(),
                switchDimming.IsOn, switchColor.IsOn, switchTemperature.IsOn, this.Dispatcher);
            Bulbs.Add(bulb);

            bulb.LampState_Hue = 0;
            bulb.LampState_Brightness = UInt32.MaxValue;
            bulb.LampState_Saturation = bulb.LampDetails_Color ? UInt32.MaxValue : 0;
            bulb.LampState_OnOff = true;
            adapter.AddBulb(bulb);
            Button_Click_Cancel(sender, e);
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            AddBulbWindow.Visibility = Visibility.Collapsed;
        }
    }
}
