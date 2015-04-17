using System.Collections.Generic;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Client.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class BuyViewModel : ViewModelBase
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

        public ICommand BuyCommand;

        private void BuyCommandExecute()
        {
            var session = App.Current.Session;
            int buyQuantity = int.Parse(BuyQuantity);
            var ret = App.Current.TheDigiMarket.CreatePurchaseOrder(session.Username, session.Password, buyQuantity);
        }

        public void OnEnter()
        {
            var session = App.Current.Session;
            PurchaseOrders = App.Current.TheDigiMarket.GetPurchaseOrders(session.Username, session.Password);
        }

        /// <summary>
        /// Initializes a new instance of the BuyViewModel class.
        /// </summary>
        public BuyViewModel()
        {
            BuyCommand  = new RelayCommand(BuyCommandExecute, () => true);
        }
    }
}