using System;
using System.Diagnostics;
using Common.Properties;

namespace Common
{
    public static class Logger
    {
        [StringFormatMethod("format")]
        public static void Log(string format, params object[] args)
        {
            var stackTrace = new StackTrace();
            var callerMember = stackTrace.GetFrame(1).GetMethod();

            var dateStr = DateTime.Now.ToString("HH:mm:ss.fff");
            var formatStr = string.Format(format, args);

            Trace.WriteLine(string.Format("{0} [{1}] {2}", dateStr, callerMember.Name, formatStr));
        }
    }
}
