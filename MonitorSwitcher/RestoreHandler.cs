using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using static MonitorSwitcher.WinApi.DataTypes;
using static MonitorSwitcher.WinApi.User32;

namespace MonitorSwitcher
{
    public class RestoreHandler
    {

        [Verb("restore", HelpText = "Restore to default settings for extended desktop.")]
        public class Options
        {
        }

        public int Run(Options o)
        {
            var res = SetDisplayConfig(0, null, 0, null, SdcFlags.Apply | SdcFlags.UseDatabaseCurrent);
            Console.WriteLine(res);
            return 0;
        }
    }
}
