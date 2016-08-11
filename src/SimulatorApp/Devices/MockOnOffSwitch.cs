using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BridgeRT;
using System.ComponentModel;
using AllJoyn.Dsb;

namespace AdapterLib
{
    public sealed class MockOnOffSwitchDevice : AdapterDevice, INotifyPropertyChanged
    {
        private Windows.UI.Core.CoreDispatcher dispatcher;
        private AdapterInterface _interfaceOn;
        private AdapterInterface _interfaceOff;
        private AdapterInterface _interfaceOnOff;
        private AdapterInterface _interfaceRemoteControl;
        private bool _onOff;

        public MockOnOffSwitchDevice(string name, string id, bool isOn, Windows.UI.Core.CoreDispatcher dispatcher) :
            base(name, "MockDevices Inc", "Mock Switch", "1", id, "")
        {
            this.dispatcher = dispatcher;
            _interfaceOnOff = CreateOnOffInterface(isOn);
            _interfaceOn = CreateOnInterface(isOn);
            _interfaceOff = CreateOffInterface(!isOn);
            _interfaceRemoteControl = CreateRemoteControllabilityInterface(true);
            AdapterBusObject abo = new AdapterBusObject("org/alljoyn/SmartSpaces/Operation/Switch");
            abo.Interfaces.Add(_interfaceOnOff);
            abo.Interfaces.Add(_interfaceOn);
            abo.Interfaces.Add(_interfaceOff);
            abo.Interfaces.Add(_interfaceRemoteControl);
            this.BusObjects.Add(abo);
            CreateEmitSignalChangedSignal();
            _onOff = isOn;
        }

        /*
            <interface name="org.alljoyn.SmartSpaces.Operation.OnControl">
                <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides capability to switch on the device."/>
                <annotation name="org.alljoyn.Bus.Secure" value="true"/>
                <property name="Version" type="q" access="read">
                    <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version."/>
                    <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
                </property>
                <method name="SwitchOn">
                    <annotation name="org.alljoyn.Bus.DocString.En" value="Switch on the device."/>
                </method>
            </interface>
        */
        private AdapterInterface CreateOnInterface(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OnControl");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            var m = new AdapterMethod("SwitchOn", "Switch on the device.", (sender, input, output) =>
            {
                OnOff = true;
            });
            iface.Methods.Add(m);
            return iface;
        }
        /*
        
        <interface name="org.alljoyn.SmartSpaces.Operation.OffControl">
            <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides the capability to switch off the device."/>
            <annotation name="org.alljoyn.Bus.Secure" value="true"/>
            <property name="Version" type="q" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version."/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
            </property>
            <method name="SwitchOff">
                <annotation name="org.alljoyn.Bus.DocString.En" value="Switch off the device."/>
            </method>
        </interface:
        */
        private AdapterInterface CreateOffInterface(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OffControl");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            var m = new AdapterMethod("SwitchOff", "Switch off the device.", (sender, input, output) =>
            {
                OnOff = false;
            });
            iface.Methods.Add(m);
            return iface;
        }

        /*
            </interface>
            <interface name="org.alljoyn.SmartSpaces.Operation.OnOffStatus">
            <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides a capability to monitor the on/off status of device."/>
            <annotation name="org.alljoyn.Bus.Secure" value="true"/>
            <property name="Version" type="q" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version."/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
            </property>
            <property name="OnOff" type="b" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="Current on/off state of the appliance. If true, the device is on state."/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
            </property>
        </interface>
        */
        private static AdapterInterface CreateOnOffInterface(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OnOffStatus");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties.Add(new AdapterAttribute("OnOff", currentValue) { COVBehavior = SignalBehavior.Always });
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.En", "Current on/off state of the appliance. If true, the device is on state.");
            return iface;
        }
        /*
         <interface name="org.alljoyn.SmartSpaces.Operation.RemoteControllability">
            <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides a capability to monitor remote control enabled/disabled status."/>
            <annotation name="org.alljoyn.Bus.Secure" value="true"/>
            <property name="Version" type="q" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version."/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
            </property>
            <property name="IsControllable" type="b" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="Status of remote controllability; true if remote controllability enabled."/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
            </property>
        </interface>
         */
        private static AdapterInterface CreateRemoteControllabilityInterface(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.RemoteControllability");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties.Add(new AdapterAttribute("IsControllable", currentValue) { COVBehavior = SignalBehavior.Always });
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.En", "Status of remote controllability; true if remote controllability enabled.");
            return iface;
        }

        public bool OnOff
        {
            get { return _onOff; }
            set
            {
                _onOff = value;
                var _ = dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnOff)));
                });
                UpdateValue(_onOff);
            }
        }

        private void UpdateValue(bool value)
        {   
            var attr = _interfaceOnOff.Properties.Where(a => a.Value.Name == "OnOff").First();
            if (attr.Value.Data != (object)value)
            {
                attr.Value.Data = value;
                SignalChangeOfAttributeValue(_interfaceOnOff, attr);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
