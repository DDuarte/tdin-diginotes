using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting;
using OpenNETCF.ORM;

namespace Server
{
    static class Program
    {
        static void Main(string[] args)
        {
            var store = DataStoreHelper.GetDataStore();

            RemotingConfiguration.Configure("Server.exe.config", false);
            SetUpConsole();

            Console.WriteLine("Listening for requests. Press Enter to exit...");
            Console.ReadLine();
        }

        private static void SetUpConsole()
        {
            Trace.Listeners.Clear();

            using (var consoleListener = new ConsoleTraceListener(true))
                Trace.Listeners.Add(consoleListener);

            Trace.AutoFlush = true;
        }
    }
}
