using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonitorSwitcher.MonitorHelpers;
using static MonitorSwitcher.DisplayHelpers;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.User32;
using static MonitorSwitcher.WinApi.Dxva2;

namespace MonitorSwitcher
{
    class Program
    {
        static async Task Watcher()
        {
            uint GetInputSource(HmonitorWrapper hWrapper)
            {
                uint currentValue = 0;
                uint maximumValue = 0;
                GetVCPFeatureAndVCPFeatureReply(
                        (IntPtr)hWrapper,
                        0x60U,  // VCP code for Input Source Select
                        0U,
                        ref currentValue,
                        ref maximumValue);
                return currentValue;
            }
            var flags = QueryDisplayFlags.OnlyActivePaths;

            var status = GetDisplayConfigBufferSizes(flags, out var numPathArrayElements, out var numModeInfoArrayElements);
            if (status != 0)
            {
                Console.WriteLine($"GetDisplaConfigBufferSizes FAILED with {status}.");
                return;
            }

            var pathArray = new DisplayConfigPathInfo[numPathArrayElements];
            var modeInfoArray = new DisplayConfigModeInfo[numModeInfoArrayElements];

            status = QueryDisplayConfig(flags, ref numPathArrayElements, pathArray, ref numModeInfoArrayElements, modeInfoArray, IntPtr.Zero);
            if (status != 0)
            {
                Console.WriteLine($"QueryDisplayConfig FAILED with {status}");
                return;
            }

            var list = pathArray.Select(path =>
            {
                var source = path.sourceInfo.modeInfoIdx < numModeInfoArrayElements ? modeInfoArray[path.sourceInfo.modeInfoIdx].sourceMode : default;
                var hmon = MonitorFromPoint(source.position, 0);
                var hwrapper = new HmonitorWrapper(hmon);
                var inpSource = GetInputSource(hwrapper);
                return new Info(hwrapper, source, inpSource);
            }).ToArray();

            while (true)
            {
                await Task.Delay(1000);
                foreach (var item in list)
                {
                    var newInput = GetInputSource(item.Wrapper);
                    if (newInput!=item.Input)
                    {
                        Console.WriteLine($"{item.Source.ToPretty()} changed source from {item.Input} to {newInput}");
                        item.Input = newInput;
                    }
                }
            }
        }

        class Info
        {
            public HmonitorWrapper Wrapper { get; }
            public DisplayConfigSourceMode Source { get; }
            public uint Input { get; set; }

            public Info(HmonitorWrapper wrapper, DisplayConfigSourceMode source, uint input = 0)
            {
                Wrapper = wrapper;
                Source = source;
                Input = input;
            }
        }

        static void Main(string[] args)
        {
            var task = Task.Run(Watcher);

            var flags = QueryDisplayFlags.OnlyActivePaths;

            var status = GetDisplayConfigBufferSizes(flags, out var numPathArrayElements, out var numModeInfoArrayElements);
            if (status != 0)
            {
                Console.WriteLine($"GetDisplaConfigBufferSizes FAILED with {status}.");
                return;
            }

            var pathArray = new DisplayConfigPathInfo[numPathArrayElements];
            var modeInfoArray = new DisplayConfigModeInfo[numModeInfoArrayElements];
            var deviceNameArray = new DisplayConfigTargetDeviceName[numModeInfoArrayElements];

            status = QueryDisplayConfig(flags, ref numPathArrayElements, pathArray, ref numModeInfoArrayElements, modeInfoArray, IntPtr.Zero);
            if (status != 0)
            {
                Console.WriteLine($"QueryDisplayConfig FAILED with {status}");
                return;
            }

            if (args.Length>0)
            {
                switch (args[0])
                {
                    case "--disable" when args.Length == 2 && int.TryParse(args[1], out var id):
                        {
                            var i = Array.FindIndex(pathArray, x => x.flags == 1 && x.targetInfo.id == id);
                            if (i < 0)
                            {
                                Console.WriteLine("unknown display id");
                            }
                            else
                            {
                                pathArray[i].flags = 0;
                                var res = SetDisplayConfig((uint)pathArray.Length, pathArray, numModeInfoArrayElements, modeInfoArray, SdcFlags.Apply | SdcFlags.UseSuppliedDisplayConfig);
                                Console.WriteLine(res);
                            }
                            //var paths = pathArray.Take((int)numPathArrayElements).Where(p => p.flags == 1).ToArray();
                            //paths[paths.Length - 1].flags = 0;
                        }
                        break;

                    case "--restore" when args.Length == 1:
                        {
                            var res = SetDisplayConfig(0, null, 0, null, SdcFlags.Apply | SdcFlags.UseDatabaseCurrent);
                            Console.WriteLine(res);
                        }
                        break;

                    default:
                        Console.WriteLine("invalid syntax");
                        break;

                }
                return;
            }

            for (int p = 0; p < numPathArrayElements; p++)
            {
                var path = pathArray[p];
                if (path.sourceInfo.statusFlags.HasFlag(DisplayConfigSourceStatus.InUse) && path.targetInfo.targetAvailable)
                {
                    var source = path.sourceInfo.modeInfoIdx < numModeInfoArrayElements ? modeInfoArray[path.sourceInfo.modeInfoIdx].sourceMode : default;
                    var target = path.targetInfo.modeInfoIdx < numModeInfoArrayElements ? modeInfoArray[path.targetInfo.modeInfoIdx].targetMode : default;

                    if (target.targetVideoSignalInfo.videoStandard == D3DkmdtVideoSignalStandard.Uninitialized)
                    {
                        Console.WriteLine($"path[{p}].flags: {path.sourceInfo.modeInfoIdx}->{path.targetInfo.modeInfoIdx} {(path.flags == 1 ? "ACTIVE" : "")}");
                        Console.WriteLine($"  src: {source.ToPretty()}");
                        Console.WriteLine($"  tgt: {target.ToPretty()}");
                        Console.WriteLine($"path[{p}].sourceInfo: {path.sourceInfo.ToPretty()}");
                        Console.WriteLine($"path[{p}].targetInfo: {path.targetInfo.ToPretty()}");

                        var pref = GetPreferredMode(path.targetInfo.adapterId, path.targetInfo.id);
                        var prefSource = new DisplayConfigSourceMode
                        {
                            height = pref.height,
                            width = pref.width
                        };
                        Console.WriteLine($"  prefTgs: {prefSource.ToPretty()}");
                        Console.WriteLine($"  prefTgs: {pref.targetMode.ToPretty()}");
                    }

                    DisplayConfigTargetDeviceName monInfo;
                    try
                    {
                        monInfo = GetMonitorAdditionalInfo(path.targetInfo.adapterId, path.targetInfo.id);
                        //Console.WriteLine($"additionalInfo: {additionalInfo.monitorFriendlyDevice}");
                    }
                    catch (Exception ex)
                    {
                        monInfo = default;
                        Console.WriteLine($"GetMonitorAdditionalInfo threw {ex.Message}");
                    }

                    var hMon = MonitorFromPoint(source.position, 0);
                    var inp = GetMonitorInputSource(hMon);

                    Console.WriteLine($"{path.targetInfo.id,8} {hMon,12} {monInfo.monitorFriendlyDeviceName ?? "unknown",12} {source.ToPretty(),-30} {path.targetInfo.outputTechnology,-20}/{inp,3} {target.targetVideoSignalInfo.videoStandard} {(path.flags == 1 ? "ACTIVE" : "")}");

                }
            }

            task.Wait();
            return;
            // get the display names for all modes
            for (var mi = 0; mi < numModeInfoArrayElements; mi++)
            {
                var modeInfo = modeInfoArray[mi];
                Console.WriteLine($"modeInfo[{mi}].id: {modeInfo.id}");
                Console.WriteLine($"modeInfo[{mi}].adapterId: {modeInfo.adapterId}");
                //Console.WriteLine($"modeInfo[{mi}].infoType: {modeInfo.infoType}");
                switch (modeInfo.infoType)
                {
                    case DisplayConfigModeInfoType.Source:
                        Console.WriteLine($"modeInfo[{mi}].sourceMode: {modeInfo.sourceMode.ToPretty()}");
                        break;

                    case DisplayConfigModeInfoType.Target:
                        Console.WriteLine($"modeInfo[{mi}].targetMode: {modeInfo.targetMode.ToPretty()}");
                        break;
                }

                if (modeInfo.infoType == DisplayConfigModeInfoType.Target)
                {
                    try
                    {
                        var deviceName = GetMonitorAdditionalInfo(modeInfo.adapterId, modeInfo.id);
                        deviceNameArray[mi] = deviceName;
                        Console.WriteLine($"additionalInfo[{mi}]: {deviceName.monitorFriendlyDeviceName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"GetMonitorAdditionalInfo threw {ex.Message}");
                    }
                }
            }
        }
    }
}
