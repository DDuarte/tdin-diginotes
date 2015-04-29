using System.Collections.Generic;
using Client.Views;
using Common;

namespace Client.ViewModels
{
    public class TransactionsViewModel : DiginotesViewModelBase
    {
        public TransactionsViewModel()
        {
            UpdateTransactions();
        }

        private IEnumerable<Transaction> _transactions;
        public IEnumerable<Transaction> Transactions
        {
            get
            {
                return _transactions;
            }
            set
            {
                _transactions = value;
                RaisePropertyChanged();
            }

        }

        public override void OnUpdate(Update update)
        {
            UpdateTransactions();
        }

        public override void OnEnter()
        {
            UpdateTransactions();
        }

        private void UpdateTransactions()
        {
            var session = App.Current.Session;

            var result = App.Current.TheDigiMarket.GetTransactions(session.Username, session.Password);

            if (!result)
                return;

            Transactions = result.Value;
        }
    }
}
