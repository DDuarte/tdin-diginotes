using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NewClient.Notifications;

namespace NewClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static MainWindow Instance { get; private set; }

        private const double TopOffset = 20;
        private const double LeftOffset = 380;
        private readonly GrowlNotifications _growlNotifications = new GrowlNotifications();

        public MainWindow()
        {
            InitializeComponent();

            _growlNotifications.Top = SystemParameters.WorkArea.Top + TopOffset;
            _growlNotifications.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - LeftOffset;

            CreateTab("LOGIN", new Login());
            CreateTab("REGISTER", new Register());

            Instance = this;
        }

        public void ShowNotification(string title, string message)
        {
            var icon = Geometry.Parse("F1 M 38,19C 48.4934,19 57,27.5066 57,38C 57,48.4934 48.4934,57 38,57C 27.5066,57 19,48.4934 19,38C 19,27.5066 27.5066,19 38,19 Z M 51,30L 42,30L 45.0857,33.0858L 39.5754,38.5962L 34.5,33.5208L 24,44.0208L 26.8284,46.8493L 34.5,39.1777L 39.5754,44.2531L 47.9142,35.9142L 51,39L 51,30 Z");
            _growlNotifications.AddNotification(new Notification { Title = title, IconData = icon, Message = message });
        }

        public void ShowChartTab()
        {
            RemoveTab("CHARTS");
            TabablzControl.SelectedItem = CreateTab("CHARTS", new DiginoteChart());
        }

        public void AfterLogin()
        {
            CreateTab("INFO", new Info());
            CreateTab("BUY", new Buy());
            CreateTab("SELL", new Sell());
            RemoveTab("LOGIN");
            RemoveTab("REGISTER");
            TabablzControl.SelectedIndex = 0;
        }

        public void AfterLogout()
        {
            CreateTab("LOGIN", new Login());
            CreateTab("REGISTER", new Register());
            RemoveTab("INFO");
            RemoveTab("BUY");
            RemoveTab("SELL");
            RemoveTab("CHARTS");
            TabablzControl.SelectedIndex = 0;
        }

        private TabItem CreateTab(string header, FrameworkElement control)
        {
            /* <TabItem Header="REGISTER">
                <views:Register Margin="8" />
               </TabItem> */

            control.Margin = new Thickness(8);
            var tabItem = new TabItem { Header = header, Content = control };

            TabablzControl.Items.Add(tabItem);
            return tabItem;
        }

        private void RemoveTab(string header)
        {
            for (var i = 0; i < TabablzControl.Items.Count; i++)
            {
                var item = TabablzControl.Items[i];
                var tabItem = item as TabItem;
                if (tabItem == null)
                    continue;

                if ((string) tabItem.Header != header)
                    continue;

                TabablzControl.Items.RemoveAt(i);
                return;
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            _growlNotifications.Close();
            base.OnClosed(e);
        }
    }
}
