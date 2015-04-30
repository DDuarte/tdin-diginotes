using Client.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
﻿using Common;

namespace Client.ViewModels
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
