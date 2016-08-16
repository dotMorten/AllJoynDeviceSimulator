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
    public sealed class MockCurrentHumidityDevice : AdapterDevice, INotifyPropertyChanged
    {
        private AdapterInterface _iface;
        private double _currentValue;
        private System.Threading.CancellationTokenSource _updateToken;

        public MockCurrentHumidityDevice(string name, string id, double currentHumidity) :
            base(name, "MockDevices Inc", "Mock Humidity", "1", id, "")
        {
            Icon = new AdapterIcon(new System.Uri("ms-appx:///Icons/Humidity.png"));
            _iface = AllJoynSimulatorApp.Devices.InterfaceCreators.CreateHumidity(currentHumidity);
            BusObjects.Add(new AdapterBusObject("org.alljoyn.SmartSpaces.Environment"));
            BusObjects[0].Interfaces.Add(_iface);
            CreateEmitSignalChangedSignal();
            _currentValue = currentHumidity;
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
                    if (!token.IsCancellationRequested)
                        UpdateValue(_currentValue);
                });
            }
        }

        public void UpdateValue(double value)
        {
            var attr = _iface.Properties.Where(a => a.Value.Name == "CurrentValue").First();
            if (attr.Value.Data != (object)value)
            {
                attr.Value.Data = value;
                base.SignalChangeOfAttributeValue(_iface, attr);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
