using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace NewClient.Views
{
    /// <summary>
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class Info : UserControl
    {
        public Info()
        {
            InitializeComponent();
            DataContext = new InfoViewModel();
        }
    }

    public class InfoViewModel : ViewModelBase
    {
        public InfoViewModel()
        {
            LogoutCommand = new RelayCommand(LogoutExecute, () => true);
            Session = App.Current.Session;
        }

        public Session Session { get; set; }

        private void LogoutExecute()
        {
            App.Current.TheDigiMarket.Logout(App.Current.Session.Username, App.Current.Session.Password);
            var mainWindow = Application.Current.Windows.Count > 0 ?
                    Application.Current.Windows[0] as MainWindow : null;

            if (mainWindow != null)
                mainWindow.AfterLogout();
        }

        public ICommand LogoutCommand { get; set; }
    }
}
