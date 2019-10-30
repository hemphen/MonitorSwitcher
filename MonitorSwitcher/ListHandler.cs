using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using static MonitorSwitcher.DisplayHelpers;
using static MonitorSwitcher.WinApi.DataTypes;

namespace MonitorSwitcher
{
    internal class ListHandler 
    {
        [Verb("list", HelpText = "List monitors")]
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('m', "monitor", Required = false, HelpText = "Monitor changes to initial inputs.")]
            public bool Monitor { get; set; }

            [Option('a', "all", Default = false, Required = false, HelpText = "Display info for all devices.")]

            public bool All { get; set; }
            [Value(0, MetaName = "id", Required = false, HelpText = "Monitor id.")]
            public int? Id { get; set; }
        }

        public int Run(Options o)
        {
            var flags = o.All ? QueryDisplayFlags.AllPaths : QueryDisplayFlags.OnlyActivePaths;

            GetDisplayConfig(flags, out var pathArray, out var modeInfoArray);

            if (o.Verbose)
            {
                PrintVerboseInfo(modeInfoArray);
            }
            else
            {
                PrintInfo(pathArray, modeInfoArray);
            }


            if (o.Monitor)
            {
                Watcher.Watch();
            }   

            return 0;
        }

        private static void PrintInfo(DisplayConfigPathInfo[] pathArray, DisplayConfigModeInfo[] modeInfoArray, bool verbose = false)
        {
            var p = -1;
            foreach (var path in pathArray)
            {
                p++;
                //if (path.sourceInfo.statusFlags.HasFlag(DisplayConfigSourceStatus.InUse) && path.targetInfo.targetAvailable)
                if (path.sourceInfo.statusFlags.HasFlag(DisplayConfigSourceStatus.InUse))
                {
                    var source = path.sourceInfo.modeInfoIdx >= 0 ? modeInfoArray[path.sourceInfo.modeInfoIdx].sourceMode : default;
                    var target = path.targetInfo.modeInfoIdx >= 0 ? modeInfoArray[path.targetInfo.modeInfoIdx].targetMode : default;

                    //if (target.targetVideoSignalInfo.videoStandard == D3DkmdtVideoSignalStandard.Uninitialized)
                    {
                        Console.WriteLine($"path[{p}].flags: {path.sourceInfo.modeInfoIdx}->{path.targetInfo.modeInfoIdx} {(path.flags == 1 ? "ACTIVE" : "")}");
                        Console.WriteLine($"  src: {source.ToPretty()}");
                        Console.WriteLine($"  tgt: {target.ToPretty()}");
                        Console.WriteLine($"path[{p}].sourceInfo: {path.sourceInfo.ToPretty()}");
                        Console.WriteLine($"path[{p}].targetInfo: {path.targetInfo.ToPretty()}");

                        if (path.targetInfo.targetAvailable)
                        {
                            var pref = GetPreferredMode(path.targetInfo.adapterId, path.targetInfo.id);
                            var prefSource = new DisplayConfigSourceMode
                            {
                                height = pref.height,
                                width = pref.width
                            };
                            Console.WriteLine($"  prefSrc: {prefSource.ToPretty()}");
                            Console.WriteLine($"  prefTgt: {pref.targetMode.ToPretty()}");
                        }
                    }

                    DisplayConfigTargetDeviceName? monInfo;
                    try
                    {
                        monInfo = GetDeviceName(path.targetInfo.adapterId, path.targetInfo.id);
                        Console.WriteLine($"additionalInfo: {monInfo.Value.monitorFriendlyDeviceName}");
                    }
                    catch (Exception ex)
                    {
                        monInfo = null;
                        Console.WriteLine($"GetMonitorAdditionalInfo threw {ex.Message}");
                    }

                    uint inp;
                    string desc;
                    using (var monitor = SafePhysicalMonitor.FromPoint(source.position))
                    {
                        inp = monitor.GetMonitorInputSource();
                        desc = monitor.Description;
                    }

                    Console.WriteLine($"{"ID",-8} {"POS",-11} {"RES",-9} {"CONNECTION",-20} {"MONITOR",-20}");
                    Console.WriteLine($"{path.targetInfo.id,-8} {$"{source.position.x},{source.position.y}",-11} {$"{source.width}x{source.height}",-9} {$"{path.targetInfo.outputTechnology}/{inp}",-20} {desc ?? "",-20}");
                    //Console.WriteLine($"{path.targetInfo.id,8} {monInfo.monitorFriendlyDeviceName ?? "unknown",12} {source.ToPretty(),-30} {$"{path.targetInfo.outputTechnology}/{inp}",-20} {target.targetVideoSignalInfo.videoStandard} {(path.flags == 1 ? "ACTIVE" : "")}");

                }
            }
        }

        private static void PrintVerboseInfo(DisplayConfigModeInfo[] modeInfoArray)
        {
            foreach (var modeInfo in modeInfoArray)
            {
                var mi = modeInfo.id;
                //Console.WriteLine($"modeInfo[{mi}].id: {modeInfo.id}");
                //Console.WriteLine($"modeInfo[{mi}].adapterId: {modeInfo.adapterId.HighPart},{modeInfo.adapterId.LowPart}");
                //Console.WriteLine($"modeInfo[{mi}].infoType: {modeInfo.infoType}");
                switch (modeInfo.infoType)
                {
                    case DisplayConfigModeInfoType.Source:
                        Console.WriteLine($"Source [{mi}]{modeInfo.sourceMode.ToPretty()}");
                        break;

                    case DisplayConfigModeInfoType.Target:
                        Console.WriteLine($"Target [{mi}]: {modeInfo.targetMode.ToPretty()}");
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
                mi++;
            }
        }


    }
}
