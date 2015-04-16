using System.Windows.Input;
using Client.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Client.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private string _username;
        public string Username
        {
            get
            {
                return _username;
                
            }
            set
            {
                _username = value;
                RaisePropertyChanged("Username");
            }
        }

        public DashboardViewModel()
        {
            BuyCommand = new RelayCommand(BuyCommandExecute, () => true);
            LogoutCommand = new RelayCommand(LogoutCommandExecute, () => true);
        }

        public ICommand BuyCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        private async void BuyCommandExecute()
        {
            NavigationService.GoTo(View.BuyOrders);
        }

        private async void LogoutCommandExecute()
        {
            App.Current.TheDigiMarket.Logout(App.Current.Session.Username, App.Current.Session.Password);
            NavigationService.GoTo(View.Login);
        }

        public void OnEnter()
        {
            Username = App.Current.Session.Username;
        }
    }
}