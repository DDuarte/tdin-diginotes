using System.Runtime.Remoting;
using System.Windows;
using Client.Utils;
using Common;
using GalaSoft.MvvmLight.Messaging;
using Remotes;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public readonly IDigiMarket TheDigiMarket;
        public Session Session = new Session("", "");
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
            RemotingConfiguration.Configure("Client.exe.config", false);

            TheDigiMarket = RemoteNew.New<IDigiMarket>();

            EventProxy = new EventProxy();
            EventProxy.MessageArrived += new MessageArrivedEvent(handleMessageArrived);
        }

        private void handleMessageArrived(string message)
        {
            if (message == "update")
                Messenger.Default.Send(Update.General);
        }
    }
}
