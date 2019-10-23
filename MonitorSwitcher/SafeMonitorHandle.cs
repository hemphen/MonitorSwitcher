using System;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.User32;
using static MonitorSwitcher.WinApi.Dxva2;

namespace MonitorSwitcher
{
    public class SafeMonitorHandle : IDisposable
    {
        private PHYSICAL_MONITOR _handle;
        private bool _isDisposed = false;

        public SafeMonitorHandle(IntPtr hMon)
        {
            _handle = GetMonitorHandle(hMon);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                DestroyPhysicalMonitor(_handle.hPhysicalMonitor);
                _isDisposed = true;
            }
        }

        public IntPtr Monitor => _handle.hPhysicalMonitor;

        public static explicit operator IntPtr(SafeMonitorHandle wrapper) => wrapper.Monitor;

        private static PHYSICAL_MONITOR GetMonitorHandle(PointL point)
        {
            IntPtr hMon = MonitorFromPoint(point, 0);
            return GetMonitorHandle(hMon);
        }

        private static PHYSICAL_MONITOR GetMonitorHandle(IntPtr hMon)
        {
            int numMonitors = 0;

            GetNumberOfPhysicalMonitorsFromHMONITOR(hMon, ref numMonitors);
            PHYSICAL_MONITOR[] pPhysicalMonitorArray = new PHYSICAL_MONITOR[numMonitors];

            // Only get first monitor
            GetPhysicalMonitorsFromHMONITOR(hMon, 1, pPhysicalMonitorArray);

            return pPhysicalMonitorArray[0];
        }
    }
}
