using System.Threading.Tasks;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Client.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {

        private readonly ViewModelLocator _viewModelLocator;

        // Properties
        public string ErrorMessage { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool AuthenticationInProgress { get; set; }

        /// <summary>
        /// Initializes a new instance of the LoginViewModel class.
        /// </summary>\
        public LoginViewModel()
        {
            AuthenticationInProgress = false;
            Login = new RelayCommand(LoginExecute, () => true);
            Register = new RelayCommand(RegisterExecute, () => true);
            _viewModelLocator = new ViewModelLocator();
        }

        public ICommand Login { get; private set; }

        private async void LoginExecute()
        {
            AuthenticationInProgress = true;
            ErrorMessage = "";

            var error = await Task.Run(() => App.Current.TheDigiMarket.Login(Username, Password));

            if (error == LoginError.None)
            {
                App.Current.Session = new Session(Username, Password);
            }

            AuthenticationInProgress = false;
            //ErrorLabel.Content = error;
        }

        public ICommand Register { get; private set; }

        private async void RegisterExecute()
        {
            AuthenticationInProgress = true;
            ErrorMessage = "";

            var error = await Task.Run(() => App.Current.TheDigiMarket.Register(Username, Password));

            if (error == RegisterError.None)
            {
                App.Current.Session = new Session(Username, Password);
                //_viewModelLocator.Main.CurrentViewModel = _viewModelLocator.Dashboard;
            }

            AuthenticationInProgress = false;
            //ErrorLabel.Content = error;
        }
    }
}