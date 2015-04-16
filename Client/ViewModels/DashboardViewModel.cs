using GalaSoft.MvvmLight;

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
            
        }

        public void OnEnter()
        {
            Username = App.Current.Session.Username;
        }
    }
}