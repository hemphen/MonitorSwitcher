using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace MonitorSwitcher
{
    static class Program
    {

        static int Main(string[] args)
        {
            return Parser
                .Default
                .ParseArguments<ListHandler.Options, ModifyHandler.Options, RestoreHandler.Options>(args)
                .MapResult<ListHandler.Options, ModifyHandler.Options, RestoreHandler.Options, int>(
                    new ListHandler().Run,
                    new ModifyHandler().Run,
                    new RestoreHandler().Run,
                    _ => 1);
        }

    }
}
