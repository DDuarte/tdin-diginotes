using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows;
using Common;
using GalaSoft.MvvmLight.Messaging;
using NewClient.Utils;
using Remotes;

namespace NewClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public readonly IDigiMarket TheDigiMarket;
        public Session Session = new Session(string.Empty, string.Empty, string.Empty);
        public EventProxy EventProxy;

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
            EventProxy.MessageArrived += new MessageArrivedEvent(handleMessageArrived);
            TheDigiMarket.MessageArrived += new MessageArrivedEvent(EventProxy.LocallyHandleMessageArrived);
        }

        private void handleMessageArrived(string message)
        {
            if (message == "update")
                Messenger.Default.Send(Update.General);
        }
    }
}
