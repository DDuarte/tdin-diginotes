using System;
using System.Diagnostics;
using System.Runtime.Remoting;

namespace Server
{
    static class Program
    {
        static void Main(string[] args)
        {
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
