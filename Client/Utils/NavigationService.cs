using GalaSoft.MvvmLight.Messaging;

namespace Client.Utils
{
    public class NavigationService
    {
        public static void GoTo(View v)
        {
            Messenger.Default.Send(v);
        }
    }
}
