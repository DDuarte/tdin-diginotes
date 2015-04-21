using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

// Implementation by Ivan Leonenko (https://github.com/IvanLeonenko/WPFGrowlNotification)

namespace NewClient.Notifications
{
    public partial class GrowlNotifications
    {
        private const byte MaxNotifications = 4;
        private int _count;
        private readonly Notifications _notifications = new Notifications();
        private readonly Notifications _buffer = new Notifications();

        public GrowlNotifications()
        {
            InitializeComponent();
            NotificationsControl.DataContext = _notifications;
        }

        public void AddNotification(Notification notification)
        {
            notification.Id = _count++;
            if (_notifications.Count + 1 > MaxNotifications)
                _buffer.Add(notification);
            else
                _notifications.Add(notification);

            //Show window if there're notifications
            if (_notifications.Count > 0 && !IsActive)
                Show();
        }

        public void RemoveNotification(Notification notification)
        {
            if (_notifications.Contains(notification))
                _notifications.Remove(notification);

            if (_buffer.Count > 0)
            {
                _notifications.Add(_buffer[0]);
                _buffer.RemoveAt(0);
            }

            // Close window if there's nothing to show
            if (_notifications.Count < 1)
                Hide();
        }

        private void NotificationWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height != 0.0)
                return;
            var element = sender as Grid;
            if (element != null)
                RemoveNotification(_notifications.First(n => n.Id == Int32.Parse(element.Tag.ToString())));
        }
    }
}
