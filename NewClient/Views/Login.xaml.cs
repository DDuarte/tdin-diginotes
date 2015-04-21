using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace NewClient.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login
    {
        public Login()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                ((dynamic)DataContext).Password = ((PasswordBox)sender).Password;
        }
    }

    public class LoginViewModel : ViewModelBase
    {
        public LoginViewModel()
        {
            AuthenticationInProgress = false;
            Login = new RelayCommand(LoginExecute, () => true);
        }

        public string Password { get; set; }

        private string _username;

        public string Username
        {
            get { return _username; }
            set { _username = value; RaisePropertyChanged(); }
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

        public ICommand Login { get; private set; }

        private async void LoginExecute()
        {
            AuthenticationInProgress = true;
            ErrorMessage = "";

            var result = await Task.Run(() => App.Current.TheDigiMarket.Login(Username, Password));

            if (result)
            {
                var user = result.Value;
                App.Current.Session = new Session(user.Name, user.Username, Password, user.Balance);
                MainWindow.Instance.AfterLogin();
            }
            else
            {
                ErrorMessage = result.Error.ToString();
            }

            AuthenticationInProgress = false;
        }
    }
}
