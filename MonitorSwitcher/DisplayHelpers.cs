using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.User32;

namespace MonitorSwitcher
{
    public class DisplayHelpers
    {
        public static DisplayConfigTargetDeviceName GetDeviceName(LUID adapterId, uint targetId)
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
