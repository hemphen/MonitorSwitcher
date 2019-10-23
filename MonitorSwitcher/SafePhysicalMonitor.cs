using System;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.Dxva2;
using static MonitorSwitcher.WinApi.User32;

namespace MonitorSwitcher
{
    public class SafePhysicalMonitor : IDisposable
    {
        private PHYSICAL_MONITOR _handle;
        private bool _isDisposed = false;

        public SafePhysicalMonitor(IntPtr hMon)
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

        public IntPtr Handle => _handle.hPhysicalMonitor;
        public string Description => _handle.szPhysicalMonitorDescription;

        public static implicit operator IntPtr(SafePhysicalMonitor monitor) => monitor.Handle;

        private static PHYSICAL_MONITOR GetMonitorHandle(IntPtr hMon)
        {
            int numMonitors = 0;

            GetNumberOfPhysicalMonitorsFromHMONITOR(hMon, ref numMonitors);
            PHYSICAL_MONITOR[] pPhysicalMonitorArray = new PHYSICAL_MONITOR[numMonitors];

            // Only get first monitor
            GetPhysicalMonitorsFromHMONITOR(hMon, 1, pPhysicalMonitorArray);

            return pPhysicalMonitorArray[0];
        }

        public static SafePhysicalMonitor FromPoint(PointL point)
        {
            var hMon = MonitorFromPoint(point, 0);
            return new SafePhysicalMonitor(hMon);
        }
    }
}
