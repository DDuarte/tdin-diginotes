using System.Windows;
using System.Windows.Controls;
using Client.ViewModels;

namespace Client.Views
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
}
