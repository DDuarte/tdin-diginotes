using System.Windows;
using System.Windows.Controls;

namespace NewClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            CreateTab("LOGIN", new Login());
            CreateTab("REGISTER", new Register());
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
            // CreateTab("SELL", new Register());
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
    }
}
