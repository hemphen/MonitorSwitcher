using System;
using static MonitorSwitcher.WinApi.Dxva2;

namespace MonitorSwitcher
{
    public static class VcpFeatureExtensions
    {
        public static bool SetMonitorInputSource(this SafePhysicalMonitor monitor, uint source) 
            => VcpFeature.SetMonitorInputSource(monitor.Handle, source);

        public static uint GetMonitorInputSource(this SafePhysicalMonitor monitor) =>
            VcpFeature.GetMonitorInputSource(monitor.Handle);

        public static bool SetDpmsControl(SafePhysicalMonitor monitor, uint command)
            => VcpFeature.SetDpmsControl(monitor.Handle, command);

        public static uint GetDpmsControl(this SafePhysicalMonitor monitor)
            => VcpFeature.GetDpmsControl(monitor.Handle);

        public static bool SetVcpValue(this SafePhysicalMonitor monitor, VcpCode vcpCode, uint value)
            => VcpFeature.SetVcpValue(monitor.Handle, vcpCode, value);

        public static uint GetVcpCalue(this SafePhysicalMonitor monitor, VcpCode vcpCode)
            => VcpFeature.GetVcpValue(monitor.Handle, vcpCode);
    }

    public enum VcpCode : uint
    {
        INPUT_SOURCE_SELECT = 0x60U,
        POWER_MODE = 0xD6U,
    }

    internal class VcpFeature
    {

        public static bool SetMonitorInputSource(IntPtr hPhysicalMonitor, uint source)
            => SetVcpValue(hPhysicalMonitor, VcpCode.INPUT_SOURCE_SELECT, source);

        public static uint GetMonitorInputSource(IntPtr hPhysicalMonitor)
            => GetVcpValue(hPhysicalMonitor, VcpCode.INPUT_SOURCE_SELECT);

        public static bool SetDpmsControl(IntPtr hPhysicalMonitor, uint command)
            => SetVcpValue(hPhysicalMonitor, VcpCode.POWER_MODE, command);

        public static uint GetDpmsControl(IntPtr hPhysicalMonitor)
            => GetVcpValue(hPhysicalMonitor, VcpCode.POWER_MODE);

        public static bool SetVcpValue(IntPtr hPhysicalMonitor, VcpCode vcpCode, uint value)
            => SetVcpValue(hPhysicalMonitor, (uint)vcpCode, value);

        public static uint GetVcpValue(IntPtr hPhysicalMonitor, VcpCode vcpCode)
            => GetVcpValue(hPhysicalMonitor, (uint)vcpCode);

        private static bool SetVcpValue(IntPtr hPhysicalMonitor, uint vcpCode, uint value)
            => SetVCPFeature(hPhysicalMonitor, vcpCode, value);

        private static uint GetVcpValue(IntPtr hPhysicalMonitor, uint vcpCode)
        {
            uint currentValue = 0;
            uint maximumValue = 0;
            GetVCPFeatureAndVCPFeatureReply(hPhysicalMonitor, 
                vcpCode, 
                0U,
                ref currentValue,
                ref maximumValue);
            return currentValue;
        }
    }
}
