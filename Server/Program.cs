using System;
using System.Runtime.Remoting;

namespace Server
{
    static class Program
    {
        static void Main(string[] args)
        {
            RemotingConfiguration.Configure("Server.exe.config", false);
            Console.WriteLine("Listening for requests. Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
