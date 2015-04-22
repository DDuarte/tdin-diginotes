using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Remotes;

namespace Server
{
    class ActionLog : IDisposable
    {
        private readonly FileStream _logFile;
        private readonly StreamWriter _writer;
        private readonly object _lock = new object();

        public ActionLog(IDigiMarket digiMarket)
        {
            _logFile = new FileStream("transaction_log.txt", FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.Read);

            digiMarket.ApplyingLogs(true);

            var reader = new StreamReader(_logFile, Encoding.UTF8);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var space = line.IndexOf(' ');
                var name = line.Substring(0, space);
                var json = line.Substring(space + 1);
                var obj = Deserialize(name, json);
                obj.Apply(digiMarket);
            }

            digiMarket.ApplyingLogs(false);

            _writer = new StreamWriter(_logFile);
        }

        private static ILogAction Deserialize(string name, string json)
        {
            switch (name)
            {
                case "NewUserAction":
                    return JsonConvert.DeserializeObject<NewUserAction>(json);
                case "QuotationChangeAction":
                    return JsonConvert.DeserializeObject<QuotationChangeAction>(json);
                case "AddFundsAction":
                    return JsonConvert.DeserializeObject<AddFundsAction>(json);
                case "OrdersSnapshot":
                    return JsonConvert.DeserializeObject<OrdersSnapshot>(json);
                default:
                    throw new ArgumentOutOfRangeException("name");
            }
        }

        public void LogAction(ILogAction action)
        {
            var name = action.GetType().Name;
            var json = JsonConvert.SerializeObject(action, Formatting.None);

            lock (_lock)
            {
                _writer.WriteLine(name + " " + json);
                _writer.Flush();
            }
        }

        public void Dispose()
        {
            _logFile.Dispose();
            _writer.Dispose();
        }
    }
}
