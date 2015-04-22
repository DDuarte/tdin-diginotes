using System.Runtime.Remoting;
using System.Windows;
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
        public Session Session = new Session(string.Empty, string.Empty, string.Empty, 0, 0);
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
            RemotingConfiguration.Configure("Client.exe.config", false);

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
