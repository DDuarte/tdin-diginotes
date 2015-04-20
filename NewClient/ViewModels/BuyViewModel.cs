using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using NewClient.Views;

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
                RaisePropertyChanged();
            }

        }

        private IEnumerable<PurchaseOrder> _openPurchaseOrders;
        public IEnumerable<PurchaseOrder> OpenPurchaseOrders
        {
            get
            {
                return _openPurchaseOrders;
                
            }
            set
            {
                _openPurchaseOrders = value;
                RaisePropertyChanged();
            }
        }

        private IEnumerable<PurchaseOrder> _closedPurchaseOrders;
        public IEnumerable<PurchaseOrder> ClosedPurchaseOrders
        {
            get
            {
                return _closedPurchaseOrders;
            }
            set
            {
                _closedPurchaseOrders = value;
                RaisePropertyChanged();
            }
        } 

        private PurchaseOrder _selectedPurchaseOrder;
        public PurchaseOrder SelectedPurchaseOrder
        {
            get
            {
                return _selectedPurchaseOrder;
            }
            set
            {
                _selectedPurchaseOrder = value;
                RaisePropertyChanged();
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
                RaisePropertyChanged();
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
                RaisePropertyChanged();
            }
        }

        public ICommand BuyCommand { get; private set; }

        private async void BuyCommandExecute()
        {
            var mainWindow = Application.Current.Windows.Count > 0 ?
                    Application.Current.Windows[0] as MainWindow : null;

            if (mainWindow == null)
                return;

            var result = await mainWindow.ShowInputAsync("Buy Diginotes", "How many?", new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented, AffirmativeButtonText = "Buy", DefaultText = "1" });

            if (result == null) //user pressed cancel
                return;

            // PurchaseNotInProgress = false;
            var session = App.Current.Session;
            int buyQuantity = int.Parse(result);
            var ret = App.Current.TheDigiMarket.CreatePurchaseOrder(session.Username, session.Password, buyQuantity);
            // PurchaseNotInProgress = true;

            await mainWindow.ShowInputAsync("Buy Diginotes", "Result " + ret);
        }

        public override void OnUpdate(Update update)
        {
            var session = App.Current.Session;
            PurchaseOrders = App.Current.TheDigiMarket.GetPurchaseOrders(session.Username, session.Password);
            OpenPurchaseOrders = PurchaseOrders.Where(order => !order.FulFilled);
            ClosedPurchaseOrders = PurchaseOrders.Where(order => order.FulFilled);
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
