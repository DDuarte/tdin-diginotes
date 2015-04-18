using GalaSoft.MvvmLight;

namespace Client.ViewModels
{
    public abstract class DiginotesViewModelBase : ViewModelBase
    {
        protected DiginotesViewModelBase()
        {
        }

        public abstract void OnEnter();
    }
}