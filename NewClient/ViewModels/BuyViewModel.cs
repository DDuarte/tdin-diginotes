using System.Collections.Generic;
using System.Windows.Input;
using NewClient.Utils;
using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace NewClient.ViewModels
{
    public class BuyViewModel : DiginotesViewModelBase
    {
        private IEnumerable<PurchaseOrder> _purchaseOrders;
        public IEnumerable<PurchaseOrder> PurchaseOrders
        {
            get
            {
                return _purchaseOrders;
            }
            set
            {
                _purchaseOrders = value;
                RaisePropertyChanged("PurchaseOrders");
            }

        }

        private string _buyQuantity;
        public string BuyQuantity
        {
            get
            {
                return _buyQuantity;

            }
            set
            {
                _buyQuantity = value;
                RaisePropertyChanged("BuyQuantity");
            }
        }

        private string _buyResultMessage;
        public string BuyResultMessage
        {
            get
            {
                return _buyResultMessage;
            }
            set
            {
                _buyResultMessage = value;
                RaisePropertyChanged("BuyResultMessage");
            }
        }

        private bool _purchaseNotInProgress;

        public bool PurchaseNotInProgress
        {
            get
            {
                return _purchaseNotInProgress;

            }
            set
            {
                _purchaseNotInProgress = value;
                RaisePropertyChanged("PurchaseNotInProgress");
            }
        }

        public ICommand BuyCommand { get; private set; }

        private void BuyCommandExecute()
        {
            PurchaseNotInProgress = false;
            var session = App.Current.Session;
            int buyQuantity = int.Parse(BuyQuantity);
            var ret = App.Current.TheDigiMarket.CreatePurchaseOrder(session.Username, session.Password, buyQuantity);
            PurchaseNotInProgress = true;
        }

        public override void OnUpdate(Update update)
        {
            var session = App.Current.Session;
            PurchaseOrders = App.Current.TheDigiMarket.GetPurchaseOrders(session.Username, session.Password);
        }

        public override void OnEnter()
        {
            var session = App.Current.Session;
            PurchaseOrders = App.Current.TheDigiMarket.GetPurchaseOrders(session.Username, session.Password);
            PurchaseNotInProgress = true;
        }

        public BuyViewModel()
        {
            BuyCommand = new RelayCommand(BuyCommandExecute, () => true);
            PurchaseNotInProgress = true;
        }
    }
}
