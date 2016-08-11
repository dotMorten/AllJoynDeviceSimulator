using DeviceProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllJoynSimulatorApp
{
    //Listens for AllJoyn notifications on the bus and raises an event when one is received
    internal class AllJoynNotificationReceiver
    {
        public delegate void OnNotificationHandler(string message);

        public event OnNotificationHandler OnNotification;

        /// Reference: https://allseenalliance.org/developers/learn/base-services/notification/interface
        private readonly IReadOnlyList<string> NOTIFICATION_PATHS = new List<string>
        {
            @"/emergency",
            @"/warning",
            @"/info",
        };

        private readonly string NOTIFICATION_INTERFACE = @"org.alljoyn.Notification";

        /// Reference: https://git.allseenalliance.org/cgit/services/notification.git/tree/cpp/src/PayloadAdapter.cc#n337
        private readonly int NOTIFICATION_TEXT_ARGUMENT_INDEX = 9;

        private ICollection<ISignal> subcribedSignals = new Collection<ISignal>();

        public AllJoynNotificationReceiver(DeviceProviders.AllJoynProvider provider = null)
        {
            DeviceProviders.AllJoynProvider Config = provider ?? new AllJoynProvider();
            Config.ServiceJoined += Config_ServiceJoined;
            Config.Start();
        }

        private void Config_ServiceJoined(IProvider sender, ServiceJoinedEventArgs args)
        {
            ServiceJoined(args.Service);
        }

        private void ServiceJoined(IService service)
        {
            if (!service.ImplementsInterface(NOTIFICATION_INTERFACE)) return;

            foreach (string path in NOTIFICATION_PATHS)
            {
                IBusObject busObject;
                IInterface @interface;
                ISignal signal;

                try
                {
                    busObject = service.Objects.First(x => x.Path == path);
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("Couldn't find bus object: " + path);
                    return;
                }

                try
                {
                    @interface = busObject.Interfaces.First(x => x.Name == NOTIFICATION_INTERFACE);
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("Couldn't find Interface: " + NOTIFICATION_INTERFACE);
                    return;
                }

                try
                {
                    signal = @interface.Signals.Single(x => x.Name.ToLower() == "notify");
                    signal.SignalRaised += AllJoynNotifier_SignalRaised;
                    subcribedSignals.Add(signal);
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("The Notification interface is not implemented correctly.");
                    return;
                }

                Debug.WriteLine(string.Format("Subscribed to {0} notifications on {1}.", path, service.Name));
            }
        }

        private void AllJoynNotifier_SignalRaised(ISignal sender, IList<object> args)
        {
            Debug.WriteLine("Received Notification signal from " + sender.Name);

            if (OnNotification == null) return;

            string message = null;

            try
            {
                var textStructs = ((IList<object>)args[NOTIFICATION_TEXT_ARGUMENT_INDEX]).Cast<AllJoynMessageArgStructure>();

                /// textStructs has type a(ss), where the first element of the struct (i.e. x[0]) is the
                /// language as per RFC 5646, while the second element (x[1]) is the message text.
                var english = textStructs.First(x => ((string)x[0]).StartsWith("en"));

                message = (string)english[1];
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in SignalRaised: " + ex.Message);
                return;
            }

            Debug.WriteLine("Notification text: " + message);

            OnNotification(message);
        }
    }
}
