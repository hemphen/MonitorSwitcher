using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using static MonitorSwitcher.DisplayHelpers;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.User32;

namespace MonitorSwitcher
{
    internal class ModifyHandler
    {
        [Verb("modify", HelpText = "Modify")]
        public class Options
        {
            [Option('v', "verbose", Default = false, Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('i', "input", Required = false, HelpText = "Change input source.")]
            public int? Input { get; set; }

            [Option('d', "disable", Required = false, HelpText = "Disable extended desktop.")]
            public bool Disable { get; set; }

            [Option('r', "restore", Required = false, HelpText = "Restore extended desktop when input is restored.")]
            public bool Restore { get; set; }

            [Value(0, MetaName = "id", Required = true, HelpText = "Monitor id.")]
            public string Id { get; set; }
        }

        private bool TryFindDisplay(string query, DisplayConfigPathInfo[] pathArray, DisplayConfigModeInfo[] modeArray, out int pathIndex)
        {
            if (int.TryParse(query, out var id))
            {
                pathIndex = Array.FindIndex(pathArray, x => x.flags == 1 && x.targetInfo.id == id);
            }
            else
            {
                pathIndex = Array.FindIndex(pathArray, path =>
                {
                    if (path.flags != 1)
                        return false;

                    if (path.sourceInfo.modeInfoIdx < 0)
                        return false;

                    var source = modeArray[path.sourceInfo.modeInfoIdx];

                    return query == $"{source.sourceMode.position.x},{source.sourceMode.position.x}";
                });
            }

            if (pathIndex < 0)
            {
                return false;
            }

            return true;
        }

        public int Run(Options o)
        {
            GetDisplayConfig(QueryDisplayFlags.OnlyActivePaths, out var pathArray, out var modeArray);

            if (!TryFindDisplay(o.Id, pathArray, modeArray, out var pathIdx))
            {
                Console.WriteLine("Invalid display id.");
                return 1;
            }

            if (o.Disable)
            {
                pathArray[pathIdx].flags = 0;
                var result = SetDisplayConfig((uint)pathArray.Length, pathArray, (uint)modeArray.Length, modeArray, SdcFlags.Apply | SdcFlags.UseSuppliedDisplayConfig);
                if (result != 0)
                {
                    Console.WriteLine($"Could not disable extended desktop for display, error code {result}.");
                    return 1;
                }
            }

            if (o.Input != null)
            {
                var path = pathArray[pathIdx];
                var sourceModeIdx = path.sourceInfo.modeInfoIdx;
                if ( sourceModeIdx < 0)
                    return 1;

                var sourceMode = modeArray[sourceModeIdx].sourceMode;
                var pos = sourceMode.position;

                using (var mon = SafePhysicalMonitor.FromPoint(pos))
                {
                    var currentInput = mon.GetMonitorInputSource();
                    var originalInput = currentInput;
                    var latestInput = currentInput;
                    if (o.Input.Value == originalInput)
                    {
                        Console.WriteLine("The monitor is already using the selected input source.");
                        return 1;
                    }

                    mon.SetMonitorInputSource((uint)o.Input.Value);

                    if (o.Restore)
                    {
                        while ((currentInput = mon.GetMonitorInputSource()) == originalInput)
                        {
                            Thread.Sleep(1000);
                        }

                        Console.WriteLine($"Monitor input source changed to {currentInput}.");
                        latestInput = currentInput;

                        while ((currentInput = mon.GetMonitorInputSource()) != originalInput)
                        {
                            if (currentInput != latestInput)
                            {
                                Console.WriteLine($"Monitor input source changed to {currentInput}, waiting for {originalInput}.");
                                latestInput = currentInput;
                            }
                            Thread.Sleep(1000);
                        }

                        Console.WriteLine($"Monitor input source changed to original value {originalInput}. Restoring extended desktop.");
                        var result = SetDisplayConfig(0, null, 0, null, SdcFlags.Apply | SdcFlags.UseDatabaseCurrent);
                        if (result != 0)
                        {
                            Console.WriteLine($"Could not restore extended desktop, error code {result}.");
                            return 1;
                        }
                    }
                }
            }

            return 0;
        }

    }
}
