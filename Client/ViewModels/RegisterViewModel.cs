using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Client.Views;
using Common;
using Remotes;

namespace Client.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        public RegisterViewModel()
        {
            AuthenticationInProgress = false;
            Register = new RelayCommand(RegisterExecute, () => true);
        }

        public string Password { get; set; }

        private string _username;

        public string Username
        {
            get { return _username; }
            set { _username = value; RaisePropertyChanged(); }
        }

        private string _name;

        public string Name
        {
            get { return _name ; }
            set { _name = value; RaisePropertyChanged(); }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; RaisePropertyChanged(); }
        }

        private bool _authenticationInProgress;

        public bool AuthenticationInProgress
        {
            get { return _authenticationInProgress; }
            set { _authenticationInProgress = value; RaisePropertyChanged(); }
        }

        public ICommand Register { get; private set; }

        private async void RegisterExecute()
        {
            AuthenticationInProgress = true;
            ErrorMessage = "";

            var result = await Task.Run(() =>
            {
                try
                {
                    return App.Current.TheDigiMarket.Register(Name, Username, Password);
                }
                catch (SocketException e)
                {
                    return new Result<User>(DigiMarketError.NetworkError);
                }
            });

            if (result)
            {
                var user = result.Value;
                App.Current.Session = new Session(user.Name, user.Username, Password, user.Balance, user.Diginotes.Count);
                MainWindow.Instance.AfterLogin();
            }
            else
            {
                if (result.Error == DigiMarketError.NetworkError)
                {
                    MainWindow.Instance.ShowNotification("Error", "Lost connection to the server, please restart the client");
                }

                ErrorMessage = result.Error.ToString();
            }

            AuthenticationInProgress = false;
        }
    }
}