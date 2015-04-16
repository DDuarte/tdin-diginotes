using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Description for LoginView.
    /// </summary>
    public partial class LoginView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the LoginView class.
        /// </summary>
        public LoginView()
        {
            InitializeComponent();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                ((dynamic)DataContext).Password = ((PasswordBox)sender).Password;
        }
    }
}
