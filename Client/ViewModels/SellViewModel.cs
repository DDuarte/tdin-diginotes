﻿using System.Collections.Generic;
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
    public class SellViewModel : DiginotesViewModelBase
    {
        public SellViewModel()
        {
            SellCommand = new RelayCommand(SellCommandExecute, () => true);
            EditCommand = new RelayCommand(EditCommandExecute, () => true);
            DeleteCommand = new RelayCommand(DeleteCommandExecute, () => true);
            SaleNotInProgress = true;

            UpdateOrders();
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

        public ICommand SellCommand { get; private set; }

        private async void SellCommandExecute()
        {
            try
            {

                var result = await MainWindow.Instance.ShowInputAsync("Sell Diginotes", "How many?",
                    new MetroDialogSettings
                    {
                        ColorScheme = MetroDialogColorScheme.Accented,
                        AffirmativeButtonText = "Sell",
                        DefaultText = "1"
                    });

                if (result == null) // user pressed cancel
                    return;

                var session = App.Current.Session;
                int sellQuantity = int.Parse(result);
                var ret = App.Current.TheDigiMarket.CreateSalesOrder(session.Username, session.Password, sellQuantity);

                if (ret.Error == DigiMarketError.NotFullfilled)
                {
                    var quotationResult = App.Current.TheDigiMarket.GetQuotation(session.Username, session.Password);
                    if (!quotationResult)
                        return;

                    result = await MainWindow.Instance.ShowInputAsync("Change quotation",
                        "Order was not fulfilled, specify new lower quotation value",
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
                        ret.Value.Id, false))
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
                    await MainWindow.Instance.ShowMessageAsync("Sell Diginotes", ret.Error.ToString());
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
                App.Current.TheDigiMarket.DeleteSaleOrder(session.Username, session.Password, SelectedSalesOrder.Id);
            }
            catch (SocketException e)
            {
                MainWindow.Instance.ShowNotification("Error", "Lost connection to the server, please restart the client");
            }
            SelectedSalesOrder = null;
        }

        public ICommand EditCommand { get; private set; }
        private void EditCommandExecute()
        {
            var session = App.Current.Session;

            try
            {
                App.Current.TheDigiMarket.UpdateSaleOrder(session.Username, session.Password, SelectedSalesOrder.Id, SelectedSalesOrder.Value);
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
            SaleNotInProgress = true;
        }

        private void UpdateOrders()
        {
            var session = App.Current.Session;

            var result = App.Current.TheDigiMarket.GetSalesOrders(session.Username, session.Password);

            if (!result)
                return;

            SalesOrders = result.Value;
        }
    }
}
