using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void LoginButton_OnClick_Click(object sender, RoutedEventArgs e)
        {
            CircularProgressBar.Visibility = Visibility.Visible;
            ErrorLabel.Content = string.Empty;

            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            var error = await Task.Run(() => App.Current.TheDigiMarket.Login(username, password));

            CircularProgressBar.Visibility = Visibility.Hidden;
            ErrorLabel.Content = error;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            CircularProgressBar.Visibility = Visibility.Visible;
            ErrorLabel.Content = string.Empty;

            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            var error = await Task.Run(() => App.Current.TheDigiMarket.Register(username, password));

            CircularProgressBar.Visibility = Visibility.Hidden;
            ErrorLabel.Content = error;
        }
    }
}
