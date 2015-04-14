using System.Runtime.Remoting;
using System.Windows;
using Common;
using Remotes;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public readonly IDigiMarket TheDigiMarket;
        public Session Session;

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
        }
    }
}
