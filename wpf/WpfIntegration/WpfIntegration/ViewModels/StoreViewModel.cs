using Flipdish.Model;
using WpfIntegration.Infrastructure;

namespace WpfIntegration.ViewModels
{
    /// <summary>
    /// Single store view model, used for displaying a single store
    /// </summary>
    class StoreViewModel : BindableBase
    {
        private Store _store;

        public StoreViewModel(Store store)
        {
            Store = store;
        }

        public Store Store
        {
            get => _store;
            set => SetProperty(ref _store, value);
        }
    }
}