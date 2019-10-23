using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonitorSwitcher.DisplayHelpers;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.User32;

namespace MonitorSwitcher
{
    class Program
    {
        static async Task Watcher()
        {
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
                var monitor = SafePhysicalMonitor.FromPoint(source.position);
                return new Info(monitor, source, monitor.GetMonitorInputSource());
            }).ToArray();

            while (true)
            {
                await Task.Delay(1000);
                foreach (var item in list)
                {
                    var newInput = item.Monitor.GetMonitorInputSource();
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
            public SafePhysicalMonitor Monitor { get; }
            public DisplayConfigSourceMode Source { get; }
            public uint Input { get; set; }

            public Info(SafePhysicalMonitor monitor, DisplayConfigSourceMode source, uint input = 0)
            {
                Monitor = monitor;
                Source = source;
                Input = input;
            }
        }

        private static int GetDisplayConfig(QueryDisplayFlags flags, out DisplayConfigPathInfo[] pathArray, out DisplayConfigModeInfo[] modeInfoArray)
        {
            pathArray = null;
            modeInfoArray = null;

            var status = GetDisplayConfigBufferSizes(flags, out var numPathElements, out var numModeInfoElements);
            if (status != 0)
            {
                Console.WriteLine($"GetDisplaConfigBufferSizes FAILED with {status}.");
                return status;
            }

            pathArray = new DisplayConfigPathInfo[numPathElements];
            modeInfoArray = new DisplayConfigModeInfo[numModeInfoElements];

            status = QueryDisplayConfig(flags, ref numPathElements, pathArray, ref numModeInfoElements, modeInfoArray, IntPtr.Zero);
            if (status != 0)
            {
                Console.WriteLine($"QueryDisplayConfig FAILED with {status}");
                return status;
            }

            return 0;
        }

        static void Main(string[] args)
        {
            //var task = Task.Run(Watcher);
            var task = Task.CompletedTask;

            var status = GetDisplayConfig(QueryDisplayFlags.AllPaths, out var pathArray, out var modeInfoArray);
            if (status != 0)
            {
                return;
            }

            var deviceNameArray = new DisplayConfigTargetDeviceName[modeInfoArray.Length];

            if (args.Length>0)
            {
                switch (args[0])
                {
                    case "--print":
                        {
                            PrintModeInfo(modeInfoArray);
                        }
                        break;

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
                                var res = SetDisplayConfig((uint)pathArray.Length, pathArray, (uint)modeInfoArray.Length, modeInfoArray, SdcFlags.Apply | SdcFlags.UseSuppliedDisplayConfig);
                                Console.WriteLine(res);
                            }
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

            var p = -1;
            foreach (var path in pathArray)
            {
                p++;
                if (path.sourceInfo.statusFlags.HasFlag(DisplayConfigSourceStatus.InUse) && path.targetInfo.targetAvailable)
                {
                    var source = path.sourceInfo.modeInfoIdx >= 0 ? modeInfoArray[path.sourceInfo.modeInfoIdx].sourceMode : default;
                    var target = path.targetInfo.modeInfoIdx >= 0 ? modeInfoArray[path.targetInfo.modeInfoIdx].targetMode : default;

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
                        Console.WriteLine($"  prefSrc: {prefSource.ToPretty()}");
                        Console.WriteLine($"  prefTgt: {pref.targetMode.ToPretty()}");
                    }

                    DisplayConfigTargetDeviceName monInfo;
                    try
                    {
                        monInfo = GetDeviceName(path.targetInfo.adapterId, path.targetInfo.id);
                        //Console.WriteLine($"additionalInfo: {additionalInfo.monitorFriendlyDevice}");
                    }
                    catch (Exception ex)
                    {
                        monInfo = default;
                        Console.WriteLine($"GetMonitorAdditionalInfo threw {ex.Message}");
                    }

                    uint inp;
                    using (var monitor = SafePhysicalMonitor.FromPoint(source.position))
                    {
                        inp = monitor.GetMonitorInputSource();
                    }

                    Console.WriteLine($"{path.targetInfo.id,8} {monInfo.monitorFriendlyDeviceName ?? "unknown",12} {source.ToPretty(),-30} {path.targetInfo.outputTechnology,-20}/{inp,3} {target.targetVideoSignalInfo.videoStandard} {(path.flags == 1 ? "ACTIVE" : "")}");

                }
            }

            task.Wait();
            return;
        }

        private static void PrintModeInfo(DisplayConfigModeInfo[] modeInfoArray)
        {
            foreach (var modeInfo in modeInfoArray)
            {
                var mi = modeInfo.id;
                Console.WriteLine($"modeInfo[{mi}].id: {modeInfo.id}");
                Console.WriteLine($"modeInfo[{mi}].adapterId: {modeInfo.adapterId.HighPart},{modeInfo.adapterId.LowPart}");
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
                        var deviceName = GetDeviceName(modeInfo.adapterId, modeInfo.id);
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
