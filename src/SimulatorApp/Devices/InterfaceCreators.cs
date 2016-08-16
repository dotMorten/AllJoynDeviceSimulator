using AllJoyn.Dsb;
using BridgeRT;
using System;
using System.Collections.Generic;

namespace AllJoynSimulatorApp.Devices
{
    internal static class InterfaceCreators
    {
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

        public static AdapterInterface CreateTemperature(double currentValue)
        {
            AdapterInterface iface = new AdapterInterface("org.alljoyn.SmartSpaces.Environment.CurrentTemperature");
            //iface.Annotations.Add("org.alljoyn.Bus.DocString.En", "This interface provides capability to represent current temperature.");
            //iface.Annotations.Add("org.alljoyn.Bus.Secure", "true");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties.Add(new AdapterAttribute("CurrentValue", currentValue) { COVBehavior = SignalBehavior.Always });
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.Type.Units", "degrees Celcius");
            iface.Properties.Add(new AdapterAttribute("Precision", 0.1d) { COVBehavior = SignalBehavior.Always });
            iface.Properties.Add(new AdapterAttribute("UpdateMinTime", (ushort)3000) { COVBehavior = SignalBehavior.Always });
            return iface;
        }

        /*
        <interface name="org.alljoyn.SmartSpaces.Environment.CurrentHumidity">
            <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides capability to represent current relative humidity."/>
            <annotation name="org.alljoyn.Bus.Secure" value="true"/>
            <property name="Version" type="q" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version"/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
            </property>
            <property name="CurrentValue" type="y" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="Current relative humidity value"/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
                <annotation name="org.alljoyn.Bus.Type.Min" value="0"/>
            </property>
            <property name="MaxValue" type="y" access="read">
                <annotation name="org.alljoyn.Bus.DocString.En" value="Maximum value allowed for represented relative humidity"/>
                <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
            </property>
        </interface>
        */

        public static AdapterInterface CreateHumidity(double currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Environment.CurrentHumidity");
            //iface.Annotations.Add("org.alljoyn.Bus.DocString.En", "This interface provides capability to represent current relative humidity.");
            //iface.Annotations.Add("org.alljoyn.Bus.Secure", "true");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties.Add(new AdapterAttribute("CurrentValue", currentValue) { COVBehavior = SignalBehavior.Always });
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.En", "Current relative humidity value");
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.Type.Min", "0");
            iface.Properties.Add(new AdapterAttribute("MaxValue", 100d) { COVBehavior = SignalBehavior.Always });
            iface.Properties[2].Annotations.Add("org.alljoyn.Bus.DocString.En", "Maximum value allowed for represented relative humidity");
            return iface;
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
        public static AdapterInterface CreateOnControl(Action switchOnAction)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OnControl");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            var m = new AdapterMethod("SwitchOn", "Switch on the device.", (sender, input, output) =>
            {
                switchOnAction();
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
        public static AdapterInterface CreateOffControl(Action switchOffAction)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OffControl");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            var m = new AdapterMethod("SwitchOff", "Switch off the device.", (sender, input, output) =>
            {
                switchOffAction();
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
        public static AdapterInterface CreateOnOffStatus(bool currentValue)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.OnOffStatus");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties.Add(new AdapterAttribute("OnOff", currentValue) { COVBehavior = SignalBehavior.Always });
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.En", "Current on/off state of the appliance. If true, the device is on state.");
            return iface;
        }

        /*
    <interface name="org.alljoyn.SmartSpaces.Operation.Brightness">
        <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides capabilities to control and monitor brightness."/>
        <annotation name="org.alljoyn.Bus.Secure" value="true"/>
        <property name="Version" type="q" access="read">
            <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version."/>
            <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
        </property>
        <property name="Brightness" type="d" access="readwrite">
            <annotation name="org.alljoyn.Bus.DocString.En" value="Brightness of the device."/>
            <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
            <annotation name="org.alljoyn.Bus.Type.Min" value="0.0"/>
            <annotation name="org.alljoyn.Bus.Type.Max" value="1.0"/>
        </property>
    </interface>
    */
        public static AdapterInterface CreateBrightness(double currentValue, Action<double> onBrightnessChanged)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.Brightness");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties.Add(new AdapterAttribute("Brightness", currentValue, (value) =>
            {
                if (!(value is double))
                    return AllJoynStatusCode.BUS_BAD_VALUE_TYPE;
                double brightness = (double)value;
                if (brightness < 0 || brightness > 1)
                    return AllJoynStatusCode.BUS_BAD_VALUE;
                onBrightnessChanged(brightness);
                return AllJoynStatusCode.Ok;
            }) { COVBehavior = SignalBehavior.Always });
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.En", "Brightness of the device.");
            return iface;
        }
        /*
     <interface name="org.alljoyn.SmartSpaces.Operation.Color">
        <annotation name="org.alljoyn.Bus.DocString.En" value="This interface provides capabilities to control and monitor color."/>
        <annotation name="org.alljoyn.Bus.Secure" value="true"/>
        <property name="Version" type="q" access="read">
            <annotation name="org.alljoyn.Bus.DocString.En" value="The interface version."/>
            <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="const"/>
        </property>
        <property name="Hue" type="d" access="readwrite">
            <annotation name="org.alljoyn.Bus.DocString.En" value="Hue of the device."/>
            <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
            <annotation name="org.alljoyn.Bus.Type.Min" value="0.0"/>
            <annotation name="org.alljoyn.Bus.Type.Max" value="360.0"/>
        </property>
        <property name="Saturation" type="d" access="readwrite">
            <annotation name="org.alljoyn.Bus.DocString.En" value="Saturation of the device."/>
            <annotation name="org.freedesktop.DBus.Property.EmitsChangedSignal" value="true"/>
            <annotation name="org.alljoyn.Bus.Type.Min" value="0.0"/>
            <annotation name="org.alljoyn.Bus.Type.Max" value="1.0"/>
        </property>
    </interface>
    */
        public static AdapterInterface CreateColor(double currentHue, double currentSaturation, Action<double> onHueChanged, Action<double> onSaturationChanged)
        {
            var iface = new AdapterInterface("org.alljoyn.SmartSpaces.Operation.Color");
            iface.Properties.Add(new AdapterAttribute("Version", (ushort)1) { COVBehavior = SignalBehavior.Never });
            iface.Properties[0].Annotations.Add("org.alljoyn.Bus.DocString.En", "The interface version");
            iface.Properties.Add(new AdapterAttribute("Hue", currentHue, (value) =>
            {
                if (!(value is double))
                    return AllJoynStatusCode.BUS_BAD_VALUE_TYPE;
                double hue = (double)value;
                if (hue < 0 || hue > 360)
                    return AllJoynStatusCode.BUS_BAD_VALUE;
                onHueChanged(hue);
                return AllJoynStatusCode.Ok;
            }) { COVBehavior = SignalBehavior.Always });
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.En", "Hue of the device.");
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.Min", "0.0");
            iface.Properties[1].Annotations.Add("org.alljoyn.Bus.DocString.Max", "360.0.");
            iface.Properties.Add(new AdapterAttribute("Saturation", currentSaturation, (value) =>
            {
                if (!(value is double))
                    return AllJoynStatusCode.BUS_BAD_VALUE_TYPE;
                double saturation = (double)value;
                if (saturation < 0 || saturation > 1)
                    return AllJoynStatusCode.BUS_BAD_VALUE;
                onSaturationChanged(saturation);
                return AllJoynStatusCode.Ok;
            })
            { COVBehavior = SignalBehavior.Always });
            iface.Properties[2].Annotations.Add("org.alljoyn.Bus.DocString.En", "Saturation of the device.");
            iface.Properties[2].Annotations.Add("org.alljoyn.Bus.DocString.Min", "0.0");
            iface.Properties[2].Annotations.Add("org.alljoyn.Bus.DocString.Max", "1.0.");
            return iface;
        }
    }
}
