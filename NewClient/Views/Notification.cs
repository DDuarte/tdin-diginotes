using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

// Implementation by Ivan Leonenko (https://github.com/IvanLeonenko/WPFGrowlNotification)

namespace NewClient.Views
{
    public class Notification : INotifyPropertyChanged
    {
        private string _message;
        public string Message
        {
            get { return _message; }

            set
            {
                if (_message == value) return;
                _message = value;
                OnPropertyChanged("Message");
            }
        }

        private int _id;
        public int Id
        {
            get { return _id; }

            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        private Geometry _iconData;
        public Geometry IconData
        {
            get { return _iconData; }
            set
            {
                if (_iconData == value) return;
                _iconData = value;
                OnPropertyChanged("IconData");
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }

            set
            {
                if (_title == value) return;
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Notifications : ObservableCollection<Notification> { }
}
