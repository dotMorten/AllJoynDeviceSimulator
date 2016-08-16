/*  
* AllJoyn Device Service Bridge for Philips Hue
*  
* Copyright (c) Morten Nielsen
* All rights reserved.  
*  
* MIT License  
*  
* Permission is hereby granted, free of charge, to any person obtaining a copy of this  
* software and associated documentation files (the "Software"), to deal in the Software  
* without restriction, including without limitation the rights to use, copy, modify, merge,  
* publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons  
* to whom the Software is furnished to do so, subject to the following conditions:  
*  
* The above copyright notice and this permission notice shall be included in all copies or  
* substantial portions of the Software.  
*  
* THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,  
* INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR  
* PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE  
* FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  
* OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER  
* DEALINGS IN THE SOFTWARE.  
*/

using AllJoyn.Dsb;
using AllJoynSimulatorApp.Devices;
using BridgeRT;
using System;
using System.Linq;

namespace AdapterLib
{
    internal class MockBulbDevice : AdapterDevice
    {
        private AdapterInterface _interfaceOnOff;
        private AdapterInterface _interfaceOn;
        private AdapterInterface _interfaceOff;
        private AdapterInterface _interfaceColor;
        private AdapterInterface _interfaceBrightness;
        // private AdapterInterface _interfaceRemoteControl;

        public MockBulbDevice(MockLightingServiceHandler handler) : base(handler.Name,
            "MockDevices Inc", "Mock Bulb", "1", handler.Id, "")
        {
            base.LightingServiceHandler = handler;
            Icon = new AdapterIcon(new System.Uri("ms-appx:///Icons/Light.png"));
            _interfaceOnOff = InterfaceCreators.CreateOnOffStatus(handler.LampState_OnOff);
            _interfaceOff = InterfaceCreators.CreateOffControl(()=> { handler.LampState_OnOff = false; });
            _interfaceOn = InterfaceCreators.CreateOnControl(() => { handler.LampState_OnOff = true; });
            _interfaceColor = InterfaceCreators.CreateColor(0d, 0d,
                (hue) => { handler.LampState_Hue = (UInt32)(hue / 360 * (UInt32.MaxValue - 1)); },
                (saturation) => { handler.LampState_Saturation = (UInt32)(saturation * (UInt32.MaxValue - 1)); });
            _interfaceBrightness = InterfaceCreators.CreateBrightness(0d, (brightness) => { handler.LampState_Brightness = (UInt32)(brightness * (UInt32.MaxValue - 1)); });
            AdapterBusObject abo = new AdapterBusObject("org/alljoyn/SmartSpaces/Operation");
            abo.Interfaces.Add(_interfaceOnOff);
            abo.Interfaces.Add(_interfaceOn);
            abo.Interfaces.Add(_interfaceOff);
            if (handler.LampDetails_Color)
                abo.Interfaces.Add(_interfaceColor);
            if (handler.LampDetails_Dimmable)
                abo.Interfaces.Add(_interfaceBrightness);
            base.CreateEmitSignalChangedSignal();
            this.BusObjects.Add(abo);
            handler.PropertyChanged += Handler_PropertyChanged;
            handler.LampStateChanged += Handler_LampStateChanged;
        }

        private void Handler_LampStateChanged(object sender, EventArgs e)
        {
            NotifySignalListener(LightingServiceHandler.LampState_LampStateChanged);
        }

        private void Handler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(MockLightingServiceHandler.LampState_OnOff))
            {
                var prop = _interfaceOnOff.Properties.Where(p => p.Value.Name == "OnOff").First();
                prop.Value.Data = ((MockLightingServiceHandler)LightingServiceHandler).LampState_OnOff;
                SignalChangeOfAttributeValue(_interfaceOnOff, prop);
            }
            else if (e.PropertyName == nameof(MockLightingServiceHandler.LampState_Saturation))
            {
                if(_interfaceColor != null)
                {
                    var prop = _interfaceColor.Properties.Where(p => p.Value.Name == "Saturation").First();
                    var s = ((MockLightingServiceHandler)LightingServiceHandler).LampState_Saturation;                    
                    prop.Value.Data = s / (UInt32.MaxValue - 1d);
                    SignalChangeOfAttributeValue(_interfaceColor, prop);
                }
            }
            else if (e.PropertyName == nameof(MockLightingServiceHandler.LampState_Hue))
            {
                if (_interfaceColor != null)
                {
                    var prop = _interfaceColor.Properties.Where(p => p.Value.Name == "Hue").First();
                    var s = ((MockLightingServiceHandler)LightingServiceHandler).LampState_Hue;
                    prop.Value.Data = s / (UInt32.MaxValue - 1d) * 360d;
                    SignalChangeOfAttributeValue(_interfaceColor, prop);
                }
            }
            else if (e.PropertyName == nameof(MockLightingServiceHandler.LampState_Brightness))
            {
                if (_interfaceBrightness != null)
                {
                    var prop = _interfaceBrightness.Properties.Where(p => p.Value.Name == "Brightness").First();
                    var s = ((MockLightingServiceHandler)LightingServiceHandler).LampState_Brightness;
                    prop.Value.Data = s / (UInt32.MaxValue - 1d);
                    SignalChangeOfAttributeValue(_interfaceBrightness, prop);
                }
            }
        }
    }
}