using System;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.User32;
using static MonitorSwitcher.WinApi.Dxva2;

namespace MonitorSwitcher
{
    public class MonitorHelpers
    {
        public class HmonitorWrapper : IDisposable
        {
            PHYSICAL_MONITOR handle;
            public HmonitorWrapper(IntPtr hMon)
            {
                handle = GetMonitorHandle(hMon);
            }

            public void Dispose()
            {
                DestroyMonitorHandle(handle.hPhysicalMonitor);  // probably leaks, should call DestroyPhysicalMonitors instead
            }

            public static explicit operator IntPtr(HmonitorWrapper wrapper)
            {
                return wrapper.handle.hPhysicalMonitor;
            }

        }

        /// <summary>
        /// get a handle for top-left monitor (usually main monitor)
        /// using native call dxva2/MonitorFromPoint
        /// </summary>
        /// <returns>a PHYSICAL_MONITOR struct, whose hPhysicalMonitor is an HMONITOR / IntPtr</returns>
        public static PHYSICAL_MONITOR GetMonitorHandle(PointL point)
        {
            // Initialize Monitor handle
            IntPtr hMon = MonitorFromPoint(
                point,  // point on monitor
                0); //flag to return primary monitor on failure

            // Get Physical Monitor from handle
            PHYSICAL_MONITOR[] pPhysicalMonitorArray = new PHYSICAL_MONITOR[8 + 256];
            GetPhysicalMonitorsFromHMONITOR(
                hMon, // monitor handle
                1,  // monitor array size
                pPhysicalMonitorArray);  // point to array with monitor
            return pPhysicalMonitorArray[0];  // probably leaky, ahem
        }

        /// <summary>
        /// get a handle for top-left monitor (usually main monitor)
        /// using native call dxva2/MonitorFromPoint
        /// </summary>
        /// <returns>a PHYSICAL_MONITOR struct, whose hPhysicalMonitor is an HMONITOR / IntPtr</returns>
        public static PHYSICAL_MONITOR GetMonitorHandle(IntPtr hMon)
        {
            // Get Physical Monitor from handle
            PHYSICAL_MONITOR[] pPhysicalMonitorArray = new PHYSICAL_MONITOR[8 + 256];
            GetPhysicalMonitorsFromHMONITOR(
                hMon, // monitor handle
                1,  // monitor array size
                pPhysicalMonitorArray);  // point to array with monitor
            return pPhysicalMonitorArray[0];  // probably leaky, ahem
        }

        /// <summary>
        /// Dispose resource handle
        /// using native call dxva2/DestroyPhysicalMonitor
        /// </summary>
        /// <param name="hMon"></param>
        public static void DestroyMonitorHandle(IntPtr hMon)
        {
            DestroyPhysicalMonitor(hMon);
        }

        /// <summary>
        /// Used to change the monitor source
        /// using native call dxva2/SetVCPFeature
        /// and VCP code 0x60-Input Source Select
        /// </summary>
        /// <param name="source">DVI = 3
        /// HDMI = 4
        /// YPbPr = 12
        /// </param>
        public static void SetMonitorInputSource(IntPtr hMon, uint source)
        {
            using (var hWrapper = new HmonitorWrapper(hMon))
            {
                var x = SetVCPFeature(
                    (IntPtr)hWrapper,
                    0x60U,  // VCP code for Input Source Select
                    source);
                Console.WriteLine(x);
                Console.ReadKey();
                x = SetVCPFeature(
                    (IntPtr)hWrapper,
                    0x60U,  // VCP code for Input Source Select
                    3);
                Console.WriteLine(x);
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
            uint currentValue = 0;
            uint maximumValue = 0;
            using (var hWrapper = new HmonitorWrapper(hMon))
            {
                GetVCPFeatureAndVCPFeatureReply(
                    (IntPtr)hWrapper,
                    0x60U,  // VCP code for Input Source Select
                    0U,
                    ref currentValue,
                    ref maximumValue);
            }
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
            using (var hWrapper = new HmonitorWrapper(hMon))
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
            using (var hWrapper = new HmonitorWrapper(hMon))
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
