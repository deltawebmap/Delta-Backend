using ArkWebMapSlaveServer;
using System;

namespace ArkWebMapSlaveServerConsole
{
    /// <summary>
    /// Nothing but a frontend console for the code.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ArkWebMapServer.MainAsync().GetAwaiter().GetResult();
        }
    }
}
