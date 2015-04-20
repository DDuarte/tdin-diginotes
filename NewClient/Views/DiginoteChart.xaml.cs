using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NewClient.Views
{
    /// <summary>
    /// Interaction logic for DiginoteChart.xaml
    /// </summary>
    public partial class DiginoteChart : UserControl
    {
        public DiginoteChart()
        {
            InitializeComponent();

            DataContext = new DiginoteChartViewModel();
        }
    }

    public class DiginoteChartViewModel
    {
        public ObservableCollection<DiginoteValue> Values { get; private set; }

        public DiginoteChartViewModel()
        {
            SelectedItem = null;
            Values = new ObservableCollection<DiginoteValue>();
            Values.Add(new DiginoteValue() { Time = DateTime.Now, Value = 1 });
            AddValues();
        }

        async void AddValues()
        {
            await Task.Delay(1000);
            Values.Add(new DiginoteValue() { Time = DateTime.Now, Value = 2 });
            await Task.Delay(1000);
            Values.Add(new DiginoteValue() { Time = DateTime.Now, Value = 1 });
            await Task.Delay(1000);
            Values.Add(new DiginoteValue() { Time = DateTime.Now, Value = 3 });
            await Task.Delay(1000);
            Values.Add(new DiginoteValue() { Time = DateTime.Now, Value = 4 });
            await Task.Delay(1000);
            Values.Add(new DiginoteValue() { Time = DateTime.Now, Value = 5 });
        }

        public object SelectedItem { get; set; }
    }

    public class DiginoteValue
    {
        public DateTime Time { get; set; }

        public decimal Value { get; set; }
    }
}
