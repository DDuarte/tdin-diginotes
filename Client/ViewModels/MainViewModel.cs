using Client.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace Client.ViewModels
{
    public class MainViewModel : DiginotesViewModelBase
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
                    CurrentViewModel = Locator.Login;
                    break; 
                }
                case View.Dashboard:
                {
                    Locator.Dashboard.OnEnter();
                    CurrentViewModel = Locator.Dashboard;
                    break;
                }
                case View.BuyOrders:
                {
                    Locator.Buy.OnEnter();
                    CurrentViewModel = Locator.Buy;
                    break;
                }
                case View.SellOrders:
                    return;
            }
        }

        public MainViewModel()
        {
            CurrentViewModel = new LoginViewModel();
            Messenger.Default.Register<View>(this, HandleViewChange);
        }

        public override void OnUpdate(Update update)
        {

        }

        public override void OnEnter()
        {
            
        }
    }
}
