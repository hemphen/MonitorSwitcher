using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.User32;

namespace MonitorSwitcher
{
    public static class DisplayHelpers
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

        public static void GetDisplayConfig(QueryDisplayFlags flags, out DisplayConfigPathInfo[] pathArray, out DisplayConfigModeInfo[] modeInfoArray)
        {
            pathArray = null;
            modeInfoArray = null;

            var error = GetDisplayConfigBufferSizes(flags, out var numPathElements, out var numModeInfoElements);
            if (error != 0)
            {
                throw new Win32Exception(error, $"GetDisplaConfigBufferSizes failed");
            }

            pathArray = new DisplayConfigPathInfo[numPathElements];
            modeInfoArray = new DisplayConfigModeInfo[numModeInfoElements];

            error = QueryDisplayConfig(flags, ref numPathElements, pathArray, ref numModeInfoElements, modeInfoArray, IntPtr.Zero);
            if (error != 0)
            {
                throw new Win32Exception(error, "QueryDisplayConfig failed");
            }
        }


    }
}
