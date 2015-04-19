using NewClient.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace NewClient.ViewModels
{
    public abstract class DiginotesViewModelBase : ViewModelBase
    {
        protected DiginotesViewModelBase()
        {
            Messenger.Default.Register<Update>(this, OnUpdate);
        }

        public abstract void OnUpdate(Update update);
        public abstract void OnEnter();
    }
}