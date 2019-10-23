using System;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.Dxva2;

namespace MonitorSwitcher
{
    public partial class MonitorHelpers
    {
        /// <summary>
        /// Used to change the monitor source
        /// using native call dxva2/SetVCPFeature
        /// and VCP code 0x60-Input Source Select
        /// </summary>
        /// <param name="source">DVI = 3
        /// HDMI = 4
        /// YPbPr = 12
        /// </param>
        /// 
        public static void SetMonitorInputSource(SafeMonitorHandle handle, uint source)
        {
            var status = SetVCPFeature(
                handle.Monitor,
                0x60U,  // VCP code for Input Source Select
                source);
        }

        public static void SetMonitorInputSource(IntPtr hMon, uint source)
        {
            using (var handle = new SafeMonitorHandle(hMon))
            {
                SetMonitorInputSource(handle, source);
            }
        }

        /// <summary>
        /// Gets Monitor source
        /// using native call dxva2/GetVCPFeatureAndVCPFeatureReply
        /// and VCP code 0x60-Input Source Select
        /// </summary>
        /// <returns>See setMonitorInputSource() for source definitions</returns>
        public static uint GetMonitorInputSource(IntPtr hMon)
        {
            using (var handle = new SafeMonitorHandle(hMon))
            {
                return GetMonitorInputSource(handle);
            }
        }

        public static uint GetMonitorInputSource(SafeMonitorHandle handle)
        {
            uint currentValue = 0;
            uint maximumValue = 0;
                GetVCPFeatureAndVCPFeatureReply(
                    handle.Monitor,
                    0x60U,  // VCP code for Input Source Select
                    0U,
                    ref currentValue,
                    ref maximumValue);
            return currentValue;
        }

        /// <summary>
        /// Runs a Dpms (power) command
        /// using native call dxva2/SetVCPFeature
        /// and VCP code 0xd6-Power Mode
        /// </summary>
        /// <param name="command">undefined = 0
        /// on = 1
        /// stby = 4
        /// phy_off = 5</param>
        public static void SetDpmsControl(IntPtr hMon, uint command)
        {
            using (var hWrapper = new SafeMonitorHandle(hMon))
            {
                SetVCPFeature(
                    (IntPtr)hWrapper,
                    0xD6U,  // VCP code for Power Mode
                    command);
            }
        }


        /// <summary>
        /// Get Dpms (power) status
        /// using native call dxva2/GetVCPFeatureAndVCPFeatureReply
        /// and VCP code 0xd6-Power Mode
        /// </summary>
        /// <returns>See setDpmsControl() for status definitions</returns>
        public static uint GetDpmsControl(IntPtr hMon)
        {
            uint currentValue = 0;
            uint maximumValue = 0;
            using (var hWrapper = new SafeMonitorHandle(hMon))
            {
                GetVCPFeatureAndVCPFeatureReply(
                    (IntPtr)hWrapper,
                    0xD6U,  // VCP code for Power Mode
                    0U,
                    ref currentValue,
                    ref maximumValue);
            }
            return currentValue;
        }
    }
}
