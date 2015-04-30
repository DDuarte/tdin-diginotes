using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Client.Views;
using Remotes;

namespace Client.ViewModels
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
            try
            {
                var result = await MainWindow.Instance.ShowInputAsync("Buy Diginotes", "How many?",
                    new MetroDialogSettings
                    {
                        ColorScheme = MetroDialogColorScheme.Accented,
                        AffirmativeButtonText = "Buy",
                        DefaultText = "1"
                    });

                if (result == null) //user pressed cancel
                    return;

                // PurchaseNotInProgress = false;
                var session = App.Current.Session;
                int buyQuantity = int.Parse(result);
                var ret = App.Current.TheDigiMarket.CreatePurchaseOrder(session.Username, session.Password, buyQuantity);
                // PurchaseNotInProgress = true;

                if (ret.Error == DigiMarketError.NotFullfilled)
                {
                    var quotationResult = App.Current.TheDigiMarket.GetQuotation(session.Username, session.Password);
                    if (!quotationResult)
                        return;

                    result = await MainWindow.Instance.ShowInputAsync("Change quotation",
                        "Order was not fulfilled, specify new higher quotation value",
                        new MetroDialogSettings
                        {
                            ColorScheme = MetroDialogColorScheme.Accented,
                            AffirmativeButtonText = "Change",
                            DefaultText = quotationResult.Value.ToString(CultureInfo.InvariantCulture)
                        });

                    if (result == null)
                        return;

                    decimal newQuotation;
                    if (!decimal.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out newQuotation))
                        return;

                    if (App.Current.TheDigiMarket.ChangeQuotation(session.Username, session.Password, newQuotation,
                        ret.Value.Id, true))
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
            catch (SocketException e)
            {
                MainWindow.Instance.ShowNotification("Error", "Lost connection to the server, please restart the client");
            }
        }

        public ICommand DeleteCommand { get; private set; }
        private void DeleteCommandExecute()
        {
            var session = App.Current.Session;

            try
            {
                App.Current.TheDigiMarket.DeletePurchaseOrder(session.Username, session.Password,
                    SelectedPurchaseOrder.Id);
            }
            catch (SocketException e)
            {
                MainWindow.Instance.ShowNotification("Error", "Lost connection to the server, please restart the client");
            }
            SelectedPurchaseOrder = null;
        }

        public ICommand EditCommand { get; private set; }

        private void EditCommandExecute()
        {
            var session = App.Current.Session;

            try
            {
                App.Current.TheDigiMarket.UpdatePurchaseOrder(session.Username, session.Password,
                    SelectedPurchaseOrder.Id, SelectedPurchaseOrder.Value);
            }
            catch (SocketException e)
            {
                MainWindow.Instance.ShowNotification("Error", "Lost connection to the server, please restart the client");
            }
        }

        public override void OnUpdate(Update update)
        {
            UpdateOrders();
        }

        public override void OnEnter()
        {
            UpdateOrders();
            PurchaseNotInProgress = true;
            SelectedPurchaseOrder = null;
        }

        private void UpdateOrders()
        {
            var session = App.Current.Session;

            var result = App.Current.TheDigiMarket.GetPurchaseOrders(session.Username, session.Password);
            if (!result)
                return;

            PurchaseOrders = result.Value;
        }

        public BuyViewModel()
        {
            BuyCommand = new RelayCommand(BuyCommandExecute, () => true);
            EditCommand = new RelayCommand(EditCommandExecute, () => true);
            DeleteCommand = new RelayCommand(DeleteCommandExecute, () => true);
            PurchaseNotInProgress = true;

            UpdateOrders();
        }
    }
}
