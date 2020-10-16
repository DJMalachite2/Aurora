using Aurora.Settings;
using OpenRGB.NET;
using OpenRGB.NET.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;
using OpenRGBColor = OpenRGB.NET.Models.Color;
using OpenRGBDevice = OpenRGB.NET.Models.Device;
using OpenRGBDeviceType = OpenRGB.NET.Enums.DeviceType;
using OpenRGBZoneType = OpenRGB.NET.Enums.ZoneType;

namespace Aurora.Devices.OpenRGB
{
    public class OpenRGBAuroraDevice : DefaultDevice
    {
        public override string DeviceName => "OpenRGB";
        protected override string DeviceInfo => string.Join(", ", _devices.Select(d => d.Name));

        private OpenRGBClient _openRgb;
        private OpenRGBDevice[] _devices;
        private OpenRGBColor[][] _deviceColors;
        private List<DK>[] _keyMappings;

        public override bool Initialize()
        {
            if (IsInitialized)
                return true;

            try
            {
                _openRgb = new OpenRGBClient(name: "Aurora");
                _openRgb.Connect();

                _devices = _openRgb.GetAllControllerData();

                _deviceColors = new OpenRGBColor[_devices.Length][];
                _keyMappings = new List<DK>[_devices.Length];

                int ramIndex = 0;                                           // INDEX for RAM DEVICE Count
                int ledLightIndex = 0;                                      // INDEX for LEDLIGHT DEVICE Count
                for (var i = 0; i < _devices.Length; i++)
                {
                    var dev = _devices[i];

                    _deviceColors[i] = new OpenRGBColor[dev.Leds.Length];
                    for (var ledIdx = 0; ledIdx < dev.Leds.Length; ledIdx++)
                        _deviceColors[i][ledIdx] = new OpenRGBColor();

                    _keyMappings[i] = new List<DK>();

                    for (int j = 0; j < dev.Leds.Length; j++)
                    {
                        //  Method for Keyboards
                        if (dev.Type == OpenRGBDeviceType.Keyboard)
                        {
                            if (OpenRGBKeyNames.Keyboard.TryGetValue(dev.Leds[j].Name, out var dk))
                            {
                                _keyMappings[i].Add(dk);
                            }
                            else
                            {
                                _keyMappings[i].Add(DK.Peripheral_Logo);
                            }
                        }
                        //  Method for Mouse
                        else if (dev.Type == OpenRGBDeviceType.Mouse)
                        {
                            if (OpenRGBKeyNames.Mouse.TryGetValue(dev.Leds[j].Name, out var dk))
                            {
                                _keyMappings[i].Add(dk);
                            }
                            else
                            {
                                _keyMappings[i].Add(DK.Peripheral_Logo);
                            }
                        }
                        //  Method for Mousemat Logo
                        else if (dev.Type == OpenRGBDeviceType.Mousemat)
                        {
                            if (OpenRGBKeyNames.MOUSEMAT_LOGO.TryGetValue(dev.Leds[j].Name, out var dk))
                            {
                                _keyMappings[i].Add(dk);
                            }
                            else
                            {
                                _keyMappings[i].Add(DK.MOUSEPAD_LOGO);
                            }
                        }
                        //  Method for Head Set Stand Logo
                        else if (dev.Type == OpenRGBDeviceType.HeadsetStand)
                        {
                            if (OpenRGBKeyNames.HEADSETSTAND_LOGO.TryGetValue(dev.Leds[j].Name, out var dk))
                            {
                                _keyMappings[i].Add(dk);
                            }
                            else
                            {
                                _keyMappings[i].Add(DK.HEADSETSTAND_LOGO);
                            }
                        }
                        //  Method for Motherboard Logo Lights
                        else if (dev.Type == OpenRGBDeviceType.Motherboard)
                        {
                            if (OpenRGBKeyNames.MOBO_LOGO.TryGetValue(dev.Leds[j].Name, out var dk))
                            {
                                _keyMappings[i].Add(dk);
                            }
                            else
                            {
                                _keyMappings[i].Add(DK.MOBO_LOGO);
                            }
                        }
                        //  Method for Cooler Logo Lights
                        else if (dev.Type == OpenRGBDeviceType.Cooler)
                        {
                            if (OpenRGBKeyNames.COOLER_LOGO.TryGetValue(dev.Leds[j].Name, out var dk))
                            {
                                _keyMappings[i].Add(dk);
                            }
                            else
                            {
                                _keyMappings[i].Add(DK.COOLER_LOGO);
                            }
                        }

                        //  Method for All other Single Logo Lights
                        else
                        {
                            _keyMappings[i].Add(DK.Peripheral_Logo);
                        }
                    }
                    //  Method for LINEAR LIGHT STRIPS 
                    uint LedOffset = 0;
                    for (int j = 0; j < dev.Zones.Length; j++)
                    {
                        if (dev.Zones[j].Type == OpenRGBZoneType.Linear)
                        {
                            for (int k = 0; k < dev.Zones[j].LedCount; k++)
                            {
                                //  Method for Mousepads with up to 20 LEDs
                                if (dev.Type == OpenRGBDeviceType.Mousemat)
                                {
                                    if (k < 20)
                                    {
                                        _keyMappings[i][(int)(LedOffset + k)] = OpenRGBKeyNames.MOUSEPAD_LIGHTS[k];
                                    }
                                }
                                else
                                //  Method for HeadsetStands with up to 20 LEDs
                                if (dev.Type == OpenRGBDeviceType.HeadsetStand)
                                {
                                    if (k < 20)
                                    {
                                        _keyMappings[i][(int)(LedOffset + k)] = OpenRGBKeyNames.HEADSETSTAND_LIGHTS[k];
                                    }
                                }
                                else
                                //  Method for Mainboards with up to 5 LEDs
                                if (dev.Type == OpenRGBDeviceType.Motherboard)
                                {
                                    if (k < 5)
                                    {
                                        _keyMappings[i][(int)(LedOffset + k)] = OpenRGBKeyNames.MOBO_LIGHTS[k];
                                    }
                                }
                                else
                                //  Method for Coolers with up to 8 LEDs
                                if (dev.Type == OpenRGBDeviceType.Cooler)
                                {
                                    if (k < 8)
                                    {
                                        _keyMappings[i][(int)(LedOffset + k)] = OpenRGBKeyNames.COOLER_LIGHTS[k];
                                    }
                                }
                                else
                                //  Method for Peripherals 
                                if (dev.Type == OpenRGBDeviceType.Unknown)
                                {
                                    if (k < 20)
                                    {
                                        _keyMappings[i][(int)(LedOffset + k)] = OpenRGBKeyNames.PERIPHERAL_LIGHTS[k];
                                    }
                                }
                                else
                                //  Method for RAM Modules with 5 LEDs
                                if (dev.Type == OpenRGBDeviceType.Dram)
                                {
                                    if (k < 5)
                                    {
                                        _keyMappings[i][(int)(LedOffset + k)] = OpenRGBKeyNames.AllRAMLights[ramIndex][k];
                                    }
                                }
                                else
                                //  Method for Ledstrips up to 200 LED Lights
                                if (dev.Type == OpenRGBDeviceType.Ledstrip)
                                {
                                    if (k < OpenRGBKeyNames.AllLedLights[ledLightIndex][j].Count)
                                    {
                                        _keyMappings[i][(int)(LedOffset + k)] = OpenRGBKeyNames.AllLedLights[ledLightIndex][j][k];
                                    }
                                }
                                else
                                {
                                //  Method for all other devices up to 32 Additional Lights
                                    if (k < 32)
                                    {
                                        _keyMappings[i][(int)(LedOffset + k)] = OpenRGBKeyNames.ADDITIONAL_LIGHTS[k];
                                    }
                                }
                            }
                        }
                        LedOffset += dev.Zones[j].LedCount;
                    }
                    //CREATING DEVICE COUNTERS
                    if (dev.Type == OpenRGBDeviceType.Dram) 
                    {
                        ramIndex++;
                    }
                    else
                    if (dev.Type == OpenRGBDeviceType.Ledstrip)
                    {
                        ledLightIndex++;
                    }
                }
            }
            catch (Exception e)
            {
                LogError("error in OpenRGB device: " + e);
                IsInitialized = false;
                return false;
            }

            IsInitialized = true;
            return IsInitialized;
        }

        public override void Shutdown()
        {
            if (!IsInitialized)
                return;

            for (var i = 0; i < _devices.Length; i++)
            {
                try
                {
                    _openRgb.UpdateLeds(i, _devices[i].Colors);
                }
                catch
                {
                    //we tried.
                }
            }

            _openRgb?.Dispose();
            _openRgb = null;
            IsInitialized = false;
        }

        public override bool UpdateDevice(Dictionary<DK, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            for (var i = 0; i < _devices.Length; i++)
            {
                //should probably store these bools somewhere when initing
                //might also add this as a property in the library
                if (!_devices[i].Modes.Any(m => m.Name == "Direct"))
                    continue;

                for (int ledIdx = 0; ledIdx < _devices[i].Leds.Length; ledIdx++)
                {
                    if (keyColors.TryGetValue(_keyMappings[i][ledIdx], out var keyColor))
                    {
                        _deviceColors[i][ledIdx] = new OpenRGBColor(keyColor.R, keyColor.G, keyColor.B);
                    }
                }

                try
                {
                    _openRgb.UpdateLeds(i, _deviceColors[i]);
                }
                catch (Exception exc)
                {
                    LogError($"Failed to update OpenRGB device {_devices[i].Name}: " + exc);
                    Reset();
                }
            }

            var sleep = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_sleep");
            if (sleep > 0)
                Thread.Sleep(sleep);

            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_sleep", 25, "Sleep for", 1000, 0);
        }
    }
}