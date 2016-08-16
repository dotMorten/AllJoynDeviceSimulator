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
    public sealed class MockCurrentTemperatureDevice : AdapterDevice, INotifyPropertyChanged
    {
        private AdapterInterface _iface;
        private double _currentValue;
        private System.Threading.CancellationTokenSource _updateToken;

        public MockCurrentTemperatureDevice(string name, string id, double currentTemperature) : 
            base(name, "MockDevices Inc", "Mock Temperature", "1", id, "")
        {
            Icon = new AdapterIcon(new System.Uri("ms-appx:///Icons/Temperature.png"));
            _iface = AllJoynSimulatorApp.Devices.InterfaceCreators.CreateTemperature(currentTemperature);
            BusObjects.Add(new AdapterBusObject("org.alljoyn.SmartSpaces.Environment"));
            BusObjects[0].Interfaces.Add(_iface);
            CreateEmitSignalChangedSignal();
            _currentValue = currentTemperature;
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
            var attr = _iface.Properties.Where(a => a.Value.Name == "CurrentValue").First();
            if (attr.Value.Data != (object)value)
            {
                attr.Value.Data = value;
                SignalChangeOfAttributeValue(_iface, attr);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
