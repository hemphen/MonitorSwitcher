using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.User32;

namespace MonitorSwitcher
{
    /// <summary>
    /// This class takes care of wrapping "Connecting and Configuring Displays(CCD) Win32 API"
    /// Original author Erti-Chris Eelmaa || easter199 at hotmail dot com
    /// Modifications made by Martin Krämer || martinkraemer84 at gmail dot com
    /// Modifications made by Michael Hemph || michael.hemph at gmail dot com
    /// </summary>
    public class DisplayHelpers
    {
        public static string MonitorFriendlyName(LUID adapterId, uint targetId)
        {
            var deviceName = new DisplayConfigTargetDeviceName
            {
                header =
                {
                    size = (uint)Marshal.SizeOf(typeof (DisplayConfigTargetDeviceName)),
                    adapterId = adapterId,
                    id = targetId,
                    type = DisplayConfigDeviceInfoType.GetTargetName
                }
            };
            var error = DisplayConfigGetDeviceInfo(ref deviceName);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);
            return deviceName.monitorFriendlyDeviceName;
        }


        public static DisplayConfigTargetDeviceName GetMonitorAdditionalInfo(LUID adapterId, uint targetId)
        {
            var deviceName = new DisplayConfigTargetDeviceName
            {
                header =
                {
                    size = (uint)Marshal.SizeOf(typeof (DisplayConfigTargetDeviceName)),
                    adapterId = adapterId,
                    id = targetId,
                    type = DisplayConfigDeviceInfoType.GetTargetName
                }
            };
            var error = DisplayConfigGetDeviceInfo(ref deviceName);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);

            return deviceName;
        }

        public static DisplayConfigTargetPreferredMode GetPreferredMode(LUID adapterId, uint targetId)
        {
            var preferredMode = new DisplayConfigTargetPreferredMode
            {
                header =
                {
                    size = (uint)Marshal.SizeOf(typeof (DisplayConfigTargetPreferredMode)),
                    adapterId = adapterId,
                    id = targetId,
                    type = DisplayConfigDeviceInfoType.GetTargetPreferredMode
                }
            };
            var error = DisplayConfigGetDeviceInfo(ref preferredMode);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);

            return preferredMode;
        }

    }
}
