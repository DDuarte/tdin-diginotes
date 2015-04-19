using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NewClient.Utils;
using NewClient.Views;

namespace NewClient.ViewModels
{
    public class InfoViewModel : DiginotesViewModelBase
    {
        public InfoViewModel()
        {
            LogoutCommand = new RelayCommand(LogoutExecute, () => true);
            Session = App.Current.Session;
        }

        public Session Session { get; set; }

        private void LogoutExecute()
        {
            App.Current.TheDigiMarket.Logout(App.Current.Session.Username, App.Current.Session.Password);
            var mainWindow = Application.Current.Windows.Count > 0 ?
                    Application.Current.Windows[0] as MainWindow : null;

            if (mainWindow != null)
                mainWindow.AfterLogout();
        }

        public ICommand LogoutCommand { get; set; }
        public override void OnUpdate(Update update)
        { 
        }

        public override void OnEnter()
        {
        }
    }
}
