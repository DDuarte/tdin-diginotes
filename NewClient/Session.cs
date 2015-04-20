using System.ComponentModel;
using System.Runtime.CompilerServices;
using NewClient.Annotations;

namespace NewClient
{
    public class Session : INotifyPropertyChanged
    {
        private string _name;
        private string _username;
        private string _password;
        private decimal _balance;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                if (value == _username) return;
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value == _password) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        public decimal Balance
        {
            get { return _balance; }
            set
            {
                if (value == _balance) return;
                _balance = value;
                OnPropertyChanged();
            }
        }

        public Session(string name, string username, string password, decimal balance)
        {
            Name = name;
            Username = username;
            Password = password;
            Balance = balance;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
