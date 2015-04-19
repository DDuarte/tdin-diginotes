using System.Threading.Tasks;
using System.Windows.Input;
using Client.Utils;
using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Client.ViewModels
{
    public class LoginViewModel : DiginotesViewModelBase
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public LoginViewModel()
        {
            AuthenticationInProgress = false;
            Login = new RelayCommand(LoginExecute, () => true);
            Register = new RelayCommand(RegisterExecute, () => true);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }

        private bool _authenticationInProgress;
        public bool AuthenticationInProgress
        {
            get
            {
                return _authenticationInProgress;
            }
            set
            {
                _authenticationInProgress = value;
                RaisePropertyChanged("AuthenticationInProgress");
            }
        }

        public ICommand Login { get; private set; }

        private async void LoginExecute()
        {
            AuthenticationInProgress = true;
            ErrorMessage = "";

            User user = null;
            var error = await Task.Run(() => App.Current.TheDigiMarket.Login(Username, Password, out user));

            if (error == LoginError.None && user != null)
            {
                App.Current.Session = new Session(Username, Password);
                NavigationService.GoTo(View.Dashboard);
            }
            else
            {
                ErrorMessage = error.ToString();
            }

            AuthenticationInProgress = false;
        }

        public ICommand Register { get; private set; }

        private async void RegisterExecute()
        {
            AuthenticationInProgress = true;
            ErrorMessage = "";

            var error = await Task.Run(() => App.Current.TheDigiMarket.Register("NoName", Username, Password));

            if (error == RegisterError.None)
            {
                App.Current.Session = new Session(Username, Password);
            }
            else
            {
                ErrorMessage = error.ToString();
            }

            AuthenticationInProgress = false;
        }

        public override void OnUpdate(Update update)
        {
            
        }

        public override void OnEnter()
        {

        }
    }
}
