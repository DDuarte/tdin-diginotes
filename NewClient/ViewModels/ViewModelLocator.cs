/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Client"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System.Runtime.InteropServices;
using NewClient.ViewModels;
using NewClient.Views;

namespace NewClient.ViewModels
{//  Example code only, feel free to copy and re-use.

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static readonly MainViewModel _main = new MainViewModel();
        private static readonly LoginViewModel _login = new LoginViewModel();
        //private static readonly DashboardViewModel _dashboard = new DashboardViewModel();
        private static readonly BuyViewModel _buy = new BuyViewModel();
        private static readonly InfoViewModel _info = new InfoViewModel();

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
        }

        /// <summary>
        /// Gets the Main property which defines the main viewmodel.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return _main;
            }
        }

        public LoginViewModel Login
        {
            get
            {
                return _login;
            }
        }

        public BuyViewModel Buy
        {
            get
            {
                return _buy;
            }
        }

        public InfoViewModel Info
        {
            get
            {
                return _info;
                
            }
        }
    }
}