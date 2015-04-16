using Client.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace Client.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        readonly static ViewModelLocator Locator = new ViewModelLocator();

        public ViewModelBase CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }
            set
            {
                if (_currentViewModel == value)
                    return;
                _currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }

        private void HandleViewChange(View v)
        {
            switch (v)
            {
                case View.Login:
                {
                    CurrentViewModel = new LoginViewModel();
                    break; 
                }
                case View.Dashboard:
                {
                    CurrentViewModel = new DashboardViewModel();
                    break;
                }
                case View.SellOrders:
                case View.BuyOrders:
                    return;
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            CurrentViewModel = new LoginViewModel();
            Messenger.Default.Register<View>(this, HandleViewChange);
        }
    }
}