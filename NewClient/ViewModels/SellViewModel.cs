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
    public class SellViewModel : DiginotesViewModelBase
    {
        public SellViewModel()
        {
            SaleCommand = new RelayCommand(SaleCommandExecute, () => true);
            EditCommand = new RelayCommand(EditCommandExecute, () => true);
            DeleteCommand = new RelayCommand(DeleteCommandExecute, () => true);
            SaleNotInProgress = true;
        }

        private IEnumerable<SalesOrder> _salesOrders;
        public IEnumerable<SalesOrder> SalesOrders
        {
            get
            {
                return _salesOrders;
            }
            set
            {
                _salesOrders = value;
                RaisePropertyChanged();
            }

        }

        private IEnumerable<SalesOrder> _openSalesOrders;
        public IEnumerable<SalesOrder> OpenSalesOrders
        {
            get
            {
                return _openSalesOrders;

            }
            set
            {
                _openSalesOrders = value;
                RaisePropertyChanged();
            }
        }

        private IEnumerable<SalesOrder> _closedSalesOrders;
        public IEnumerable<SalesOrder> ClosedSalesOrders
        {
            get
            {
                return _closedSalesOrders;
            }
            set
            {
                _closedSalesOrders = value;
                RaisePropertyChanged();
            }
        }

        private SalesOrder _selectedSalesOrder;
        public SalesOrder SelectedSalesOrder
        {
            get
            {
                return _selectedSalesOrder;
            }
            set
            {
                _selectedSalesOrder = value;
                RaisePropertyChanged();
            }
        }

        private string _salesResultMessage;
        public string SalesResultMessage
        {
            get
            {
                return _salesResultMessage;
            }
            set
            {
                _salesResultMessage = value;
                RaisePropertyChanged();
            }
        }

        private bool _saleNotInProgress;
        public bool SaleNotInProgress
        {
            get
            {
                return _saleNotInProgress;

            }
            set
            {
                _saleNotInProgress = value;
                RaisePropertyChanged();
            }
        }

        public ICommand SaleCommand { get; private set; }

        private async void SaleCommandExecute()
        {
            var result = await MainWindow.Instance.ShowInputAsync("Buy Diginotes", "How many?",
                new MetroDialogSettings { ColorScheme = MetroDialogColorScheme.Accented,
                    AffirmativeButtonText = "Buy",
                    DefaultText = "1" });

            if (result == null) //user pressed cancel
                return;

            // PurchaseNotInProgress = false;
            var session = App.Current.Session;
            int buyQuantity = int.Parse(result);
            var ret = App.Current.TheDigiMarket.CreatePurchaseOrder(session.Username, session.Password, buyQuantity);
            // PurchaseNotInProgress = true;

            await MainWindow.Instance.ShowInputAsync("Buy Diginotes", "Result " + ret);
        }

        public ICommand DeleteCommand { get; private set; }
        private void DeleteCommandExecute()
        {
            var session = App.Current.Session;
            App.Current.TheDigiMarket.DeletePurchaseOrder(session.Username, session.Password, SelectedSalesOrder.Id);
            SelectedSalesOrder = null;
        }

        public ICommand EditCommand { get; private set; }

        private void EditCommandExecute()
        {
            var session = App.Current.Session;
            App.Current.TheDigiMarket.UpdatePurchaseOrder(session.Username, session.Password, SelectedSalesOrder.Id, SelectedSalesOrder.Value);
        }


        public override void OnUpdate(Update update)
        {
            var session = App.Current.Session;

            var result = App.Current.TheDigiMarket.GetSalesOrders(session.Username, session.Password);

            if (!result)
                return;

            SalesOrders = result.Value;
            OpenSalesOrders = SalesOrders.Where(order => order.Fulfilled == false);
            ClosedSalesOrders = SalesOrders.Where(order => order.Fulfilled);
        }

        public override void OnEnter()
        {
            var session = App.Current.Session;

            var result = App.Current.TheDigiMarket.GetSalesOrders(session.Username, session.Password);

            SalesOrders = result.Value;
            SaleNotInProgress = true;
        }
    }
}
