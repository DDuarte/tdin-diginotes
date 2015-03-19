using System;
using System.Diagnostics;

namespace Common
{
    public static class Logger
    {
        public static void Log(string function, string format, params object[] args)
        {
            var dateStr = DateTime.Now.ToString("HH:mm:ss.fff");
            var formatStr = string.Format(format, args);

            Trace.WriteLine(string.Format("{0} [{1}] {2}", dateStr, function, formatStr));
        }
    }
}
