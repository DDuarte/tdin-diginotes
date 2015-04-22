using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using NewClient.Views;
using Remotes;

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

            if (ret.Error == DigiMarketError.NotFullfilled)
            {
                result = await MainWindow.Instance.ShowInputAsync("Buy Diginotes",
                    "Order was not fulfilled, specify new quotation value",
                    new MetroDialogSettings
                    {
                        ColorScheme = MetroDialogColorScheme.Accented,
                        AffirmativeButtonText = "Buy",
                        DefaultText =
                            App.Current.TheDigiMarket.GetQuotation(session.Username, session.Password)
                                .Value.ToString(CultureInfo.InvariantCulture)

                    });

                if (result == null)
                    return;

                decimal newQuotation;
                if (!decimal.TryParse(result, out newQuotation)) return;

                if (App.Current.TheDigiMarket.ChangeQuotation(session.Username, session.Password, newQuotation, true))
                {
                    MainWindow.Instance.ShowNotification("Info", "Quotation successfully changed");
                }
                else
                {
                    MainWindow.Instance.ShowNotification("Error", "Error changing quotation");
                }
            }
            else
            {
                await MainWindow.Instance.ShowMessageAsync("Buy Diginotes", ret.Error.ToString());
            }
            
        }

        public ICommand DeleteCommand { get; private set; }
        private void DeleteCommandExecute()
        {
            var session = App.Current.Session;
            App.Current.TheDigiMarket.DeletePurchaseOrder(session.Username, session.Password, SelectedPurchaseOrder.Id);
            SelectedPurchaseOrder = null;
        }

        public ICommand EditCommand { get; private set; }

        private void EditCommandExecute()
        {
            var session = App.Current.Session;
            App.Current.TheDigiMarket.UpdatePurchaseOrder(session.Username, session.Password, SelectedPurchaseOrder.Id, SelectedPurchaseOrder.Value);
        }


        public override void OnUpdate(Update update)
        {
            var session = App.Current.Session;

            var result = App.Current.TheDigiMarket.GetPurchaseOrders(session.Username, session.Password);
            if (!result)
                return;

            PurchaseOrders = result.Value;
            OpenPurchaseOrders = PurchaseOrders.Where(order => !order.FulFilled);
            ClosedPurchaseOrders = PurchaseOrders.Where(order => order.FulFilled);
        }

        public override void OnEnter()
        {
            var session = App.Current.Session;

            var result = App.Current.TheDigiMarket.GetPurchaseOrders(session.Username, session.Password);
            if (!result)
                return;

            PurchaseOrders = result.Value;
            PurchaseNotInProgress = true;
            SelectedPurchaseOrder = null;
        }

        public BuyViewModel()
        {
            BuyCommand = new RelayCommand(BuyCommandExecute, () => true);
            EditCommand = new RelayCommand(EditCommandExecute, () => true);
            DeleteCommand = new RelayCommand(DeleteCommandExecute, () => true);
            PurchaseNotInProgress = true;
        }
    }
}
