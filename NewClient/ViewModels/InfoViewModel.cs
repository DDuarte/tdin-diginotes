using System;
using System.Windows;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight.Command;
using NewClient.Views;
using Remotes;

namespace NewClient.ViewModels
{
    public class InfoViewModel : DiginotesViewModelBase
    {
        public InfoViewModel()
        {
            LogoutCommand = new RelayCommand(LogoutExecute, () => true);
            AddFundsCommand = new RelayCommand(AddFundsExecute, () => true);
            ShowQuotation = new RelayCommand(ShowQuotationExecute, () => true);
            Session = App.Current.Session;
            UpdateBalanceString();
        }

        public Session Session { get; set; }

        private void LogoutExecute()
        {
            App.Current.TheDigiMarket.Logout(App.Current.Session.Username, App.Current.Session.Password);
            MainWindow.Instance.AfterLogout();
        }

        private void AddFundsExecute()
        {
            App.Current.TheDigiMarket.AddFunds(App.Current.Session.Username, App.Current.Session.Password, 10, 10);
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

            }
        }

        public override void OnEnter()
        {
        }
    }
}
