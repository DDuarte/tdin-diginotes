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
        }

        public Session Session { get; set; }

        private void LogoutExecute()
        {
            App.Current.TheDigiMarket.Logout(App.Current.Session.Username, App.Current.Session.Password);
            var mainWindow = Application.Current.Windows.Count > 0 ?
                    Application.Current.Windows[0] as MainWindow : null;

            if (mainWindow != null)
                mainWindow.AfterLogout();
        }

        private void AddFundsExecute()
        {
            App.Current.TheDigiMarket.AddFunds(App.Current.Session.Username, App.Current.Session.Password, 10);
        }

        private void ShowQuotationExecute()
        {
            var mainWindow = Application.Current.Windows.Count > 0 ?
                    Application.Current.Windows[0] as MainWindow : null;

            if (mainWindow != null)
                mainWindow.ShowChartTab();
        }

        public ICommand LogoutCommand { get; private set; }

        public ICommand AddFundsCommand { get; private set; }

        public ICommand ShowQuotation { get; private set; }

        public override void OnUpdate(Update update)
        {
            switch (update)
            {
                case Update.General:
                    break;
                case Update.Balance:
                    var result = App.Current.TheDigiMarket.GetBalance(Session.Username, Session.Password);
                    if (result)
                        Session.Balance = result.Value;
                    break;
            }
        }

        public override void OnEnter()
        {
        }
    }
}
