using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Common;

namespace Client.ViewModels
{
    public class DiginoteChartViewModel : DiginotesViewModelBase
    {
        public ObservableCollection<DiginoteValue> Values { get; private set; }

        public DiginoteChartViewModel()
        {
            SelectedItem = null;
            Values = new ObservableCollection<DiginoteValue>();
            GetHistory();
        }

        private void GetHistory()
        {
            var result = App.Current.TheDigiMarket.GetQuotationHistory(App.Current.Session.Username,
                App.Current.Session.Password);

            if (result == null)
                return;

            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    Values.Clear();
                    foreach (var pair in result)
                    {
                        Values.Add(new DiginoteValue { Time = pair.Key, Value = pair.Value });
                    }
                }));
        }

        public object SelectedItem { get; set; }

        public override void OnUpdate(Update update)
        {
            if (update != Update.Quotation)
                return;

            GetHistory();
        }

        public override void OnEnter()
        {
        }
    }

    public class DiginoteValue
    {
        public DateTime Time { get; set; }

        public decimal Value { get; set; }
    }
}
