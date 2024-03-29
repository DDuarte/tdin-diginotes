﻿using System;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight.Command;
using Client.Views;
using Remotes;

namespace Client.ViewModels
{
    public class InfoViewModel : DiginotesViewModelBase
    {
        private string _quotation;

        public String Quotation
        {
            get
            {
                return _quotation;
            }
            set
            {
                _quotation = value;
                RaisePropertyChanged();
            }
        }

        public InfoViewModel()
        {
            Quotation = "inf €";
            LogoutCommand = new RelayCommand(LogoutExecute, () => true);
            AddFundsCommand = new RelayCommand(AddFundsExecute, () => true);
            ShowQuotation = new RelayCommand(ShowQuotationExecute, () => true);
            Session = App.Current.Session;
            UpdateBalanceString();
            UpdateQuotation();
        }

        public Session Session { get; set; }

        private void LogoutExecute()
        {
            try
            {
                App.Current.TheDigiMarket.Logout(App.Current.Session.Username, App.Current.Session.Password);

            }
            catch (SocketException e)
            {
                MainWindow.Instance.ShowNotification("Error", "Lost connection to the server, please restart the client");
            }

            MainWindow.Instance.AfterLogout();
        }

        private void AddFundsExecute()
        {
            try
            {
                App.Current.TheDigiMarket.AddFunds(App.Current.Session.Username, App.Current.Session.Password, 10, 10);

            }
            catch (SocketException e)
            {
                MainWindow.Instance.ShowNotification("Error", "Lost connection to the server, please restart the client");
            }
        }

        private void ShowQuotationExecute()
        {
            MainWindow.Instance.ShowChartTab();
        }

        public ICommand LogoutCommand { get; private set; }

        public ICommand AddFundsCommand { get; private set; }

        public ICommand ShowQuotation { get; private set; }

        private void UpdateBalanceString()
        {
            Balance = string.Format("{0} € / {1} dn", Session.Balance, Session.DiginoteCount);
        }

        private string _balance;

        public string Balance
        {
            get { return _balance; }
            set
            {
                if (_balance == value) return;
                _balance = value;
                RaisePropertyChanged();
            }
        }

        public override void OnUpdate(Update update)
        {
            switch (update)
            {
                case Update.General:
                    break;
                case Update.Balance:
                {
                    var result = App.Current.TheDigiMarket.GetBalance(Session.Username, Session.Password);
                    if (result)
                    {
                        Session.Balance = result.Value;
                        UpdateBalanceString();
                    }
                    break;
                }
                case Update.Diginotes:
                {
                    var result = App.Current.TheDigiMarket.GetDiginotes(Session.Username, Session.Password);
                    if (result)
                    {
                        Session.DiginoteCount = result.Value.Count;
                        UpdateBalanceString();
                    }
                    break;
                }
                case Update.Quotation:
                {
                    UpdateQuotation();
                    break;
                }
            }
        }

        private void UpdateQuotation()
        {
            var result = App.Current.TheDigiMarket.GetQuotation(Session.Username, Session.Password);
            if (result)
                Quotation = result.Value + " €";
        }

        public override void OnEnter()
        {
        }
    }
}
