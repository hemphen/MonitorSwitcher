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
    public static class Watcher
    {
        class MonitorInputInfo
        {
            public SafePhysicalMonitor Monitor { get; }
            public DisplayConfigSourceMode Source { get; }
            public uint Input { get; set; }

            public MonitorInputInfo(SafePhysicalMonitor monitor, DisplayConfigSourceMode source, uint input = 0)
            {
                Monitor = monitor;
                Source = source;
                Input = input;
            }
        }

        public static void Watch() => WatchAsync().Wait();

        public static async Task WatchAsync()
        {
            var flags = QueryDisplayFlags.OnlyActivePaths;
            GetDisplayConfig(flags, out var pathArray, out var modeInfoArray);

            var list = pathArray.Select(path =>
            {
                var source = path.sourceInfo.modeInfoIdx>=0 ? modeInfoArray[path.sourceInfo.modeInfoIdx].sourceMode : default;
                var monitor = SafePhysicalMonitor.FromPoint(source.position);
                return new MonitorInputInfo(monitor, source, monitor.GetMonitorInputSource());
            }).ToArray();

            while (true)
            {
                await Task.Delay(1000);
                foreach (var item in list)
                {
                    var newInput = item.Monitor.GetMonitorInputSource();
                    if (newInput != item.Input)
                    {
                        Console.WriteLine($"{item.Source.ToPretty()} changed source from {item.Input} to {newInput}");
                        item.Input = newInput;
                    }
                }
            }
        }
    }

}
