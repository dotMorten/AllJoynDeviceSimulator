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
            InitializeNotification();  
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



        #region Notifications

        private void InitializeNotification()
        {
            AllJoynBusAttachment bus = new AllJoynBusAttachment();
            bus.AboutData.DefaultDescription = "AllJoyn Device Simulator";
            bus.AboutData.ModelNumber = "Notification Consumer";

            org.alljoyn.Notification.NotificationWatcher watcher = new org.alljoyn.Notification.NotificationWatcher(bus);
            watcher.Start();
            watcher.Added += Watcher_Added;
        }

        private async void Watcher_Added(org.alljoyn.Notification.NotificationWatcher sender, AllJoynServiceInfo args)
        {
            var consumer = await org.alljoyn.Notification.NotificationConsumer.JoinSessionAsync(args, sender);
            consumer.Consumer.Signals.NotifyReceived += Signals_NotifyReceived;
        }

        private void Signals_NotifyReceived(org.alljoyn.Notification.NotificationSignals sender, org.alljoyn.Notification.NotificationNotifyReceivedEventArgs args)
        {
            var e = new NotificationEventArgs()
            {
                Version = args.Arg,
                MsgId = args.Arg2,
                MsgType = args.Arg3,
                DeviceId = args.Arg4,
                DeviceName = args.Arg5,
                AppId = new Guid(args.Arg6.ToArray()),
                AppName = args.Arg7,
                Attributes = args.Arg8,
                CustomAttributes = args.Arg9,
            };
            Dictionary<string, string> languages = new Dictionary<string, string>();
            foreach(var item in args.Arg10)
            {
                languages.Add(item.Value1, item.Value2);
            }
            e.Text = languages;
            NotificationRecieved?.Invoke(this, e);
        }

        public event EventHandler<NotificationEventArgs> NotificationRecieved;

        public sealed class NotificationEventArgs : EventArgs
        {
            public UInt16 Version { get; set; }
            public int MsgId { get; set; }
            public UInt16 MsgType { get; set; }
            public string DeviceId { get; set; }
            public string DeviceName { get; set; }
            public Guid AppId { get; set; }
            public string AppName { get; set; }
            public IDictionary<int, object> Attributes { get; set; }
            public IDictionary<string, string> CustomAttributes { get; set; }
            public IDictionary<string, string> Text { get; set; }
        }


        #endregion Notifications
    }
}
