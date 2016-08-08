using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BridgeRT;
using System.ComponentModel;

/*<interface name="org.alljoyn.SmartSpaces.Environment.CurrentTemperature">
  <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides capability to represent current temperature."/>
  <annotation name="org.alljoyn.Bus.Secure" value="true"/>
  <property name="Version" type="q" access="read">
      <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version."/>
      <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
  </property>
  <property name="CurrentValue" type="d" access="read">
      <annotation name="org.alljoyn.Bus.DocString.En" value="Current temperature expressed in Celsius."/>
      <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
      <annotation name="org.alljoyn.Bus.Type.Units" value="degrees Celsius"/>
  </property>
  <property name="Precision" type="d" access="read">
      <annotation name="org.alljoyn.Bus.DocString.En" value="The precision of the CurrentValue property. i.e. the number of degrees Celsius the actual power consumption must change before CurrentValue is updated."/>
      <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
      <annotation name="org.alljoyn.Bus.Type.Units" value="degrees Celsius"/>
  </property>
  <property name="UpdateMinTime" type="q" access="read">
      <annotation name="org.alljoyn.Bus.DocString.En" value="The minimum time between updates of the CurrentValue property in milliseconds."/>
      <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
      <annotation name="org.alljoyn.Bus.Type.Units" value="milliseconds"/>
  </property>
</interface>*/

namespace AdapterLib
{
    public sealed class MockCurrentTemperatureDevice : AdapterDevice, INotifyPropertyChanged
    {
        private Adapter _bridge;
        private IAdapterProperty _property;
        private double _currentValue;
        private System.Threading.CancellationTokenSource _updateToken;

        public MockCurrentTemperatureDevice(Adapter bridge, string name, string id, double currentTemperature) : 
            base(name, "MockDevices Inc", "Mock Temperature", "1", id, "")
        {
            _bridge = bridge;
            _property = CreateInterface("Temperature", currentTemperature);
            Properties.Add(_property);
            CreateEmitSignalChangedSignal();
            _currentValue = currentTemperature;
        }

        private static IAdapterProperty CreateInterface(string objectPath, double currentValue)
        {
            AdapterProperty property = new AdapterProperty(objectPath, "org.alljoyn.SmartSpaces.Environment.CurrentTemperature");
            property.Attributes.Add(new AdapterAttribute("Version", (ushort)1, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Never });
            property.Attributes.Add(new AdapterAttribute("CurrentValue", currentValue, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            property.Attributes[1].Annotations.Add("org.alljoyn.Bus.Type.Units", "degrees Celcius");
            property.Attributes.Add(new AdapterAttribute("Precision", 0.1d, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            property.Attributes.Add(new AdapterAttribute("UpdateMinTime", (ushort)1000, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            return property;
        }

        private void CreateEmitSignalChangedSignal()
        {
            // change of value signal
            AdapterSignal changeOfAttributeValue = new AdapterSignal(Constants.CHANGE_OF_VALUE_SIGNAL);
            changeOfAttributeValue.Params.Add(new AdapterValue(Constants.COV__PROPERTY_HANDLE, null));
            changeOfAttributeValue.Params.Add(new AdapterValue(Constants.COV__ATTRIBUTE_HANDLE, null));
            Signals.Add(changeOfAttributeValue);
        }

        public double CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = Math.Round(value, 1);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentValue)));

                //Throttle updates to 1sec so we don't saturate the bus
                if (_updateToken != null) _updateToken.Cancel();
                var token = _updateToken = new System.Threading.CancellationTokenSource();
                Task.Delay(1000).ContinueWith(t =>
                {
                    if(!token.IsCancellationRequested)
                        UpdateValue(_currentValue);
                });
            }
        }

        private void UpdateValue(double value)
        {
            var attr = _property.Attributes.Where(a => a.Value.Name == "CurrentValue").First();
            if (attr.Value.Data != (object)value)
            {
                attr.Value.Data = value;
                _bridge.SignalChangeOfAttributeValue(this, _property, attr);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
