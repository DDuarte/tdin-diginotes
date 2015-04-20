using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace NewClient.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Register : UserControl
    {
        public Register()
        {
            InitializeComponent();
            DataContext = new RegisterViewModel();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                ((dynamic)DataContext).Password = ((PasswordBox)sender).Password;
        }
    }

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

            var error = await Task.Run(() => App.Current.TheDigiMarket.Register(Name, Username, Password));

            if (error == RegisterError.None)
            {
                App.Current.Session = new Session(Name, Username, Password, 0);

                var mainWindow = Application.Current.Windows.Count > 0 ?
                    Application.Current.Windows[0] as MainWindow : null;

                if (mainWindow != null)
                    mainWindow.AfterLogin();
            }
            else
            {
                ErrorMessage = error.ToString();
            }

            AuthenticationInProgress = false;
        }
    }
}
