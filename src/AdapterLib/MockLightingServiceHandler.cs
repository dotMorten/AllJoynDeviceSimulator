/*  
* AllJoyn Device Service Bridge for Mocked lights
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

using BridgeRT;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

// This class implements the AllJoyn Lighting Service Framework for the Hue Bulbs

namespace AdapterLib
{
    public sealed class MockLightingServiceHandler : ILSFHandler, INotifyPropertyChanged
    {
        private Windows.UI.Core.CoreDispatcher dispatcher;
        public MockLightingServiceHandler(string name, string id, bool isDimmable, bool supportsColor, bool supportsTemperature, Windows.UI.Core.CoreDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            Name = name;
            Id = id;
            LampDetails_Color = supportsColor;
            LampDetails_ColorRenderingIndex = 80;
            LampDetails_Dimmable = isDimmable;
            LampDetails_HasEffects = true;
            LampDetails_IncandescentEquivalent = 60;
            LampDetails_LampBaseType = (uint)1;
            LampDetails_LampBeamAngle = 130;
            LampDetails_LampID = id;
            LampDetails_LampType = (uint) 1;
            LampDetails_Make = (uint)AdapterLib.LsfEnums.LampMake.MAKE_OEM1;
            LampDetails_MaxLumens = 1000;
            LampDetails_MaxTemperature = (uint)(supportsTemperature ? 9000 : 2800);
            LampDetails_MaxVoltage = 120;
            LampDetails_MinTemperature = 2800;
            colorTemp = kelvinToUInt(LampDetails_MinTemperature);
            LampDetails_MinVoltage = 100;
            LampDetails_Model = 1;
            LampDetails_Type = (uint)AdapterLib.LsfEnums.DeviceType.TYPE_LAMP;
            LampDetails_VariableColorTemp = supportsTemperature;
            LampDetails_Version = 1;
            LampDetails_Wattage = 7;
            if (!supportsColor) saturation = 0;
        }
        [DataMember]
        public bool LampDetails_Color
        {
            get; private set;
        }

        public uint LampDetails_ColorRenderingIndex { get; private set; }

        public bool LampDetails_Dimmable { get; private set; }

        public bool LampDetails_HasEffects { get; private set; }

        public uint LampDetails_IncandescentEquivalent { get; private set; }

        public uint LampDetails_LampBaseType { get; private set; }

        public uint LampDetails_LampBeamAngle { get; private set; }

        public string LampDetails_LampID { get; private set; }

        public uint LampDetails_LampType { get; private set; }

        public uint LampDetails_Make { get; private set; }

        public uint LampDetails_MaxLumens { get; private set; }

        public uint LampDetails_MaxTemperature { get; private set; }

        public uint LampDetails_MaxVoltage { get; private set; }

        public uint LampDetails_MinTemperature { get; private set; }

        public uint LampDetails_MinVoltage { get; private set; }

        public uint LampDetails_Model { get; private set; }

        public uint LampDetails_Type { get; private set; }

        public bool LampDetails_VariableColorTemp { get; private set; }

        public uint LampDetails_Version { get; private set; }

        public uint LampDetails_Wattage { get; private set; }

        public uint LampParameters_BrightnessLumens
        {
            get
            {
                if (!LampState_OnOff)
                    return 0;
                return (UInt32)(LampDetails_MaxLumens * ( LampState_Brightness / (double)MaxUIntValue));
            }
        }
        public uint LampParameters_EnergyUsageMilliwatts
        {
            get
            {
                if (!LampState_OnOff)
                    return 0;
                return (UInt32)(LampDetails_Wattage * (LampState_Brightness / (double)MaxUIntValue));
            }
        } 

        public uint LampParameters_Version { get; private set; }

        public uint[] LampService_LampFaults { get; private set; }

        public uint LampService_LampServiceVersion { get; private set; }

        public uint LampService_Version { get; private set; }

        private uint brightness = MaxUIntValue;

        public uint LampState_Brightness
        {
            //1..254
            get { return brightness; }
            set
            {
                if (LampDetails_Dimmable)
                {
                    brightness = value;
                    OnPropertyChanged();
                }
            }
        }
        private static UInt32 kelvinToUInt(double kelvin)
        {
            return (UInt32)((kelvin - 1000) / 19000 * MaxUIntValue);
        }
        private uint colorTemp = kelvinToUInt(2800);

        public uint LampState_ColorTemp
        {
            get
            {
                return colorTemp;
            }

            set
            {
                if (LampDetails_VariableColorTemp)
                {
                    if (value > kelvinToUInt(LampDetails_MaxTemperature))
                        value = kelvinToUInt(LampDetails_MaxTemperature);
                    else if(value < kelvinToUInt(LampDetails_MinTemperature))
                        value = kelvinToUInt(LampDetails_MinTemperature);
                    colorTemp = value;
                    OnPropertyChanged();
                }
            }
        }

        private uint hue = 0;
        public uint LampState_Hue
        {
            get
            {
                return hue;
            }

            set
            {
                if (LampDetails_Color)
                {
                    hue = value;
                    OnPropertyChanged();
                }
            }
        }

        private AdapterSignal _LampStateChanged = new AdapterSignal(Constants.LAMP_STATE_CHANGED_SIGNAL_NAME);

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (dispatcher.HasThreadAccess)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                if (propertyName == nameof(LampState_Hue) ||
                    propertyName == nameof(LampState_Brightness) ||
                    propertyName == nameof(LampState_Saturation))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorFullBrightness)));
                }
                if (propertyName == nameof(LampState_Brightness) ||
                    propertyName == nameof(LampState_OnOff))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((nameof(LampParameters_BrightnessLumens))));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((nameof(LampParameters_EnergyUsageMilliwatts))));
                }
            }
            else
            {
                var _ = dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => OnPropertyChanged(propertyName));
            }
        }


        public IAdapterSignal LampState_LampStateChanged
        {
            get
            {
                return _LampStateChanged;
            }
        }

        private bool OnOff = true;
        public bool LampState_OnOff
        {
            get
            {
                return OnOff;
            }

            set
            {
                if (OnOff != value)
                {
                    OnOff = value;
                    OnPropertyChanged();
                }
            }
        }

        private uint saturation = uint.MaxValue;
        public uint LampState_Saturation
        {
            get
            {
                return saturation;
            }

            set
            {
                if (LampDetails_Color)
                {
                    saturation = value;
                    OnPropertyChanged();
                }
            }
        }

        public uint LampState_Version
        {
            get; private set;
        }
        public string Id { get; }
        public string Name { get; }

        public uint ClearLampFault(uint InLampFaultCode, out uint LampResponseCode, out uint OutLampFaultCode)
        {
            InLampFaultCode = 0;
            LampResponseCode = 0;
            OutLampFaultCode = 0;
            return 0; //TODO
        }

        public uint LampState_ApplyPulseEffect(BridgeRT.State FromState, BridgeRT.State ToState, uint Period, uint Duration, uint NumPulses, ulong Timestamp, out uint LampResponseCode)
        {
            LampResponseCode = 0;
            return 1; //TODO
        }

        /// <summary>
        /// Change the state of the lamp at the specified time, between the specified OnOff, Brightness, Hue,
        /// Saturation, and ColorTemp values. Pulse for the specified number of times, at the specified duration
        /// </summary>
        /// <param name="FromState">Current state of the lamp to transition from</param>
        /// <param name="ToState">New state of the lamp to transition to</param>
        /// <param name="Period">Time period(in ms) to transition over to new state</param>
        /// <param name="Duration">Time period(in ms) to remain in new state</param>
        /// <param name="NumPulses">Number of pulses</param>
        /// <param name="Timestamp">Timestamp (in ms) of when to start the pulses</param>
        private async void ApplyPulseEffectAsync(BridgeRT.State FromState, BridgeRT.State ToState, uint Period, uint Duration, uint NumPulses, ulong Timestamp)
        {
            uint response;
            await System.Threading.Tasks.Task.Delay((int)Timestamp).ConfigureAwait(false);
            TransitionLampState(0, FromState, 0, out response);
            for (int i = 0; i < NumPulses; i++)
            {
                TransitionLampState(0, ToState, Period, out response);
                await System.Threading.Tasks.Task.Delay((int)(Period + Duration)).ConfigureAwait(false);
                TransitionLampState(0, FromState, Period, out response);
                await System.Threading.Tasks.Task.Delay((int)(Period + Duration)).ConfigureAwait(false);
            }
        }

        public uint TransitionLampState(ulong Timestamp, BridgeRT.State NewState, uint TransitionPeriod, out uint LampResponseCode)
        {
            TransitionLampStateAsync(Timestamp, NewState, TransitionPeriod);
            LampResponseCode = 0;
            return 0; //TODO
        }
        System.Threading.CancellationTokenSource transitionCancellation;
        private object transitionLock = new object();
        private async void TransitionLampStateAsync(ulong Timestamp, BridgeRT.State NewState, uint TransitionPeriod)
        {
            if (Timestamp > 0)
                await Task.Delay((int)Timestamp);
            System.Threading.CancellationToken token = System.Threading.CancellationToken.None;
            lock (transitionLock)
            {
                if (transitionCancellation != null)
                {
                    //Cancel any ongoing transitions
                    transitionCancellation.Cancel();
                }
                transitionCancellation = new System.Threading.CancellationTokenSource();
                token = transitionCancellation.Token;
            }
            var steps = Math.Floor(30d / (1000d / TransitionPeriod));
            var stepdelay = TransitionPeriod / steps;
            var startHue = LampState_Hue;
            var startBrightness = LampState_Brightness;
            var startSaturation = LampState_Saturation;
            var startTemp = LampState_ColorTemp;
            if (NewState.IsOn.HasValue && NewState.IsOn.Value)
                LampState_OnOff = true;
            if(NewState.ColorTemp.HasValue && LampDetails_VariableColorTemp)
            {
                //Clip to supported temperatures
                if (NewState.ColorTemp.Value > kelvinToUInt(LampDetails_MaxTemperature))
                    NewState.ColorTemp = kelvinToUInt(LampDetails_MaxTemperature);
                else if (NewState.ColorTemp.Value < kelvinToUInt(LampDetails_MinTemperature))
                    NewState.ColorTemp = kelvinToUInt(LampDetails_MinTemperature);
            }
            for (int i = 1; i < steps; i++)
            {
                if(NewState.Hue.HasValue && LampDetails_Color)
                {
                    var inc = (NewState.Hue.Value - (double)startHue) / steps;
                    this.hue = (uint)(startHue + inc * i);
                    OnPropertyChanged(nameof(LampState_Hue));
                }
                if (NewState.Brightness.HasValue && LampDetails_Dimmable)
                {
                    var inc = (NewState.Brightness.Value - (double)startBrightness) / steps;
                    this.brightness = (uint)(startBrightness + inc * i);
                    OnPropertyChanged(nameof(LampState_Brightness));
                }
                if (NewState.Saturation.HasValue && LampDetails_Color)
                {
                    var inc = (NewState.Saturation.Value - (double)startSaturation) / steps;
                    this.saturation = (uint)(startSaturation + inc * i);
                    OnPropertyChanged(nameof(LampState_Saturation));
                }
                if (NewState.ColorTemp.HasValue && LampDetails_VariableColorTemp)
                {
                    var inc = (NewState.ColorTemp.Value - (double)startTemp) / steps;
                    this.colorTemp = (uint)(startTemp + inc * i);
                    OnPropertyChanged(nameof(LampState_ColorTemp));
                }
                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(ColorFullBrightness));
                await Task.Delay((int)stepdelay);
                if (token.IsCancellationRequested)
                    return;
            }
            if (NewState.Hue.HasValue && LampDetails_Color)
            {
                this.hue = NewState.Hue.Value;
                OnPropertyChanged(nameof(LampState_Hue));
            }
            if (NewState.Brightness.HasValue && LampDetails_Dimmable)
            {
                this.brightness = NewState.Brightness.Value;
                OnPropertyChanged(nameof(LampState_Brightness));
            }
            if (NewState.Saturation.HasValue && LampDetails_Color)
            {
                this.saturation = NewState.Saturation.Value;
                OnPropertyChanged(nameof(LampState_Saturation));
            }
            if (NewState.ColorTemp.HasValue && LampDetails_VariableColorTemp)
            {
                this.colorTemp = NewState.ColorTemp.Value;
                OnPropertyChanged(nameof(LampState_ColorTemp));
            }
            OnPropertyChanged(nameof(Color));
            OnPropertyChanged(nameof(ColorFullBrightness));
            if (NewState.IsOn.HasValue && !NewState.IsOn.Value)
                LampState_OnOff = false;
        }

        private const uint MaxUIntValue = UInt32.MaxValue - 1;
        public Windows.UI.Color Color
        {
            get
            {
                var h = (float)(this.hue * 360d / MaxUIntValue);
                var s = (float)(this.saturation / (double)MaxUIntValue);
                var b = this.brightness;
                if (!LampDetails_Color) { h = 0; s = 0; }
                if (!LampDetails_Dimmable) { b = MaxUIntValue; }
                var brightnessFactor = (this.saturation / (double)MaxUIntValue) + 1;
                var b2 = (float)(b / (double)MaxUIntValue / brightnessFactor);
                return FromAhsb(h, s, b2);
            }
        }
        public Windows.UI.Color ColorFullBrightness
        {
            get
            {
                if (!LampDetails_Color)
                    return Windows.UI.Colors.White;
                return FromAhsb(
                    (float)(this.hue * 360d / MaxUIntValue),
                    (float)(this.saturation / (double)MaxUIntValue),
                   .5f);
            }
        }

        public static Windows.UI.Color FromAhsb(float hue, float saturation, float brightness)
        {
            if (0f > hue
                || 360f < hue)
            {
                throw new ArgumentOutOfRangeException(
                    "hue",
                    hue,
                    "Value must be within a range of 0 - 360.");
            }

            if (0f > saturation
                || 1f < saturation)
            {
                throw new ArgumentOutOfRangeException(
                    "saturation",
                    saturation,
                    "Value must be within a range of 0 - 1.");
            }

            if (0f > brightness
                || 1f < brightness)
            {
                throw new ArgumentOutOfRangeException(
                    "brightness",
                    brightness,
                    "Value must be within a range of 0 - 1.");
            }

            if (0 == saturation)
            {
                return Windows.UI.Color.FromArgb(
                                    255,
                                    (byte)Convert.ToInt32(brightness * 255),
                                    (byte)Convert.ToInt32(brightness * 255),
                                    (byte)Convert.ToInt32(brightness * 255));
            }

            float fMax, fMid, fMin;
            int iSextant; 
              byte iMax, iMid, iMin;

            if (0.5 < brightness)
            {
                fMax = brightness - (brightness * saturation) + saturation;
                fMin = brightness + (brightness * saturation) - saturation;
            }
            else
            {
                fMax = brightness + (brightness * saturation);
                fMin = brightness - (brightness * saturation);
            }

            iSextant = (int)Math.Floor(hue / 60f);
            if (300f <= hue)
            {
                hue -= 360f;
            }

            hue /= 60f;
            hue -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2)
            {
                fMid = (hue * (fMax - fMin)) + fMin;
            }
            else
            {
                fMid = fMin - (hue * (fMax - fMin));
            }

            iMax = (byte)(fMax * 255);
            iMid = (byte)(fMid * 255);
            iMin = (byte)(fMin * 255);

            switch (iSextant)
            {
                case 1:
                    return Windows.UI.Color.FromArgb(255, iMid, iMax, iMin);
                case 2:
                    return Windows.UI.Color.FromArgb(255, iMin, iMax, iMid);
                case 3:
                    return Windows.UI.Color.FromArgb(255, iMin, iMid, iMax);
                case 4:
                    return Windows.UI.Color.FromArgb(255, iMid, iMin, iMax);
                case 5:
                    return Windows.UI.Color.FromArgb(255, iMax, iMin, iMid);
                default:
                    return Windows.UI.Color.FromArgb(255, iMax, iMid, iMin);
            }
        }

        [DataContract]
        private class SerializerClass
        {
            [DataMember]
            public string Id { get; set; }
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public bool IsDimmable { get; set; }
            [DataMember]
            public bool SupportsColor { get; set; }
            [DataMember]
            public bool SupportsTemperature { get; set; }
            [DataMember]
            public UInt32 Hue { get; set; }
            [DataMember]
            public UInt32 Saturation { get; set; }
            [DataMember]
            public UInt32 Brightness { get; set; }
            [DataMember]
            public UInt32 ColorTemp { get; set; }
            [DataMember]
            public bool IsOn { get; set; }
        }

        public string ToJson()
        {
            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(SerializerClass));
            var sc = new SerializerClass()
            {
                Id = this.Id,
                Name = Name,
                IsDimmable = LampDetails_Dimmable,
                SupportsColor = LampDetails_Color,
                SupportsTemperature = LampDetails_VariableColorTemp,
                Hue = LampState_Hue,
                Saturation = LampState_Saturation,
                Brightness = LampState_Brightness,
                ColorTemp = LampState_ColorTemp,
                IsOn = LampState_OnOff,
            };
            using (MemoryStream ms = new MemoryStream())
            {
                s.WriteObject(ms, sc);
                return System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
        }
        public static MockLightingServiceHandler FromJson(string json, Windows.UI.Core.CoreDispatcher dispatcher)
        {
            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(SerializerClass));
            using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            {
                var sc = (SerializerClass)s.ReadObject(ms);
                var mlsh = new MockLightingServiceHandler(sc.Name, sc.Id, sc.IsDimmable, sc.SupportsColor, sc.SupportsTemperature, dispatcher);
                mlsh.LampState_Hue = sc.Hue;
                mlsh.LampState_Brightness = sc.Brightness;
                mlsh.LampState_Saturation = sc.Saturation;
                mlsh.LampState_ColorTemp= sc.ColorTemp;
                mlsh.LampState_OnOff = sc.IsOn;
                return mlsh;
            }
        }
    }
}
