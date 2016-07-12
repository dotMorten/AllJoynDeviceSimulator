using AdapterLib;
using BridgeRT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using Windows.System.Threading;

namespace AllJoynSimulatorApp
{
    public class AllJoynDeviceManager
    {
        private static AllJoynDeviceManager instance;

        public static AllJoynDeviceManager Current
        {
            get
            {
                if (instance == null)
                    instance = new AllJoynDeviceManager();
                return instance;
            }
        }

        private Adapter adapter;

        public DsbBridge dsbBridge { get; private set; }

        public Task StartupTask { get; private set; }

        public async Task Shutdown()
        {
            foreach (var bulb in Bulbs.ToArray())
                RemoveBulb(bulb);
            await Task.Delay(1000); //Give it some time to announce devices lost
            StartupTask = null;
            dsbBridge.Shutdown();
            dsbBridge.Dispose();
            dsbBridge = null;
            adapter.Shutdown();
            adapter = null;
            instance = null;
            await Task.Delay(1000); //Give it some time to announce DSB lost
        }

        private AllJoynDeviceManager()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            StartupTask = ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction action) =>
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

        public void AddBulb(MockLightingServiceHandler bulb)
        {
            _Bulbs.Add(bulb);
            adapter.AddBulb(bulb);
        }
        public void RemoveBulb(MockLightingServiceHandler bulb)
        {
            _Bulbs.Remove(bulb);
            adapter.RemoveBulb(bulb);
        }

        ObservableCollection<MockLightingServiceHandler> _Bulbs = new ObservableCollection<MockLightingServiceHandler>();
        public IEnumerable<MockLightingServiceHandler> Bulbs { get { return _Bulbs; } }
    }
}
