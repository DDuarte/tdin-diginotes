using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows;
using Common;
using GalaSoft.MvvmLight.Messaging;
using Remotes;

namespace NewClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public readonly IDigiMarket TheDigiMarket;
        public Session Session = new Session(string.Empty, string.Empty, string.Empty, 0);
        public readonly EventProxy EventProxy;

        public new static App Current
        {
            get
            {
                return Application.Current as App;
            }
        }

        public App()
        {
            RemotingConfiguration.Configure("NewClient.exe.config", false);

            TheDigiMarket = RemoteNew.New<IDigiMarket>();

            EventProxy = new EventProxy();
            EventProxy.MessageArrived += HandleMessageArrived;
            TheDigiMarket.MessageArrived += EventProxy.LocallyHandleMessageArrived;
        }

        private void HandleMessageArrived(Update update)
        {
            Messenger.Default.Send(update);
        }
    }
}
