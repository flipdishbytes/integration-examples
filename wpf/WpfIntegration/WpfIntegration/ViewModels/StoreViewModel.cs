using Flipdish.Model;
using WpfIntegration.Infrastructure;

namespace WpfIntegration.ViewModels
{
    class StoreViewModel : BindableBase
    {
        private Store _store;

        public Store Store
        {
            get => _store;
            set => SetProperty(ref _store, value);
        }

        public StoreViewModel(Store store)
        {
            Store = store;
        }
    }
}