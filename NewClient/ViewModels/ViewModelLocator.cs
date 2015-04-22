/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Client"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using NewClient.Views;

namespace NewClient.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static readonly MainViewModel _main = new MainViewModel();
        private static readonly LoginViewModel _login = new LoginViewModel();
        private static readonly BuyViewModel _buy = new BuyViewModel();
        private static readonly InfoViewModel _info = new InfoViewModel();
        private static readonly SellViewModel _sell = new SellViewModel();
        private static readonly DiginoteChartViewModel _diginoteChart = new DiginoteChartViewModel();

        public MainViewModel Main { get { return _main; } }
        public LoginViewModel Login { get { return _login; } }
        public BuyViewModel Buy { get { return _buy; } }
        public InfoViewModel Info { get { return _info; } }
        public SellViewModel Sell { get { return _sell; } }
        public DiginoteChartViewModel DiginoteChart { get { return _diginoteChart; } }
    }
}
