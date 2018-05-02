using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Flipdish.Api;
using Flipdish.Client;
using Flipdish.Model;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;

namespace WpfIntegration.ViewModels
{
    class StoresViewModel : BindableBase, IViewModel
    {
        public event EventHandler<AppNavigationEventArgs> RequestNavigation;

        private readonly StoresApi _storesApi;
        private readonly string _accessToken;
        private const int StoresPerPage = 15;
        private int _pageIndex = 1;
        private int _totalPages;
        private StoreViewModel _selectedStore;
        private string _searchQuery;

        public StoresViewModel(string accessToken)
        {
            _accessToken = accessToken;
            Stores = new ObservableCollection<StoreViewModel>();
            PreviousPageCommand = new RelayCommand(ExecutePreviousPageCommand, m => _pageIndex > 1);
            NextPageCommand = new RelayCommand(ExecuteNextPageCommand, m => _pageIndex < _totalPages); 
            SelectStoreCommand = new RelayCommand(ExecuteSelectStoreCommand, m => SelectedStore != null);
            SearchCommand = new RelayCommand(ExecuteSearchCommand);

            //We require an Authorization header on our requests with the Bearer token
            var defaultHeaders = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {accessToken}" }
            };

            //Create a configuration for the API
            var configuration = new Configuration
            {
                BasePath = AppSettings.Settings.Endpoint,
                DefaultHeader = defaultHeaders
            };

            //Create a new instance of API with the configuration
            _storesApi = new StoresApi(configuration);
        }
        
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }
        public StoreViewModel SelectedStore
        {
            get => _selectedStore;
            set => SetProperty(ref _selectedStore, value);
        }
        public ObservableCollection<StoreViewModel> Stores { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand SelectStoreCommand { get; }
        public ICommand SearchCommand { get; }

        private async void ExecutePreviousPageCommand(object obj)
        {
            _pageIndex--;
            await UpdateStores();
        }

        private async void ExecuteNextPageCommand(object obj)
        {
            _pageIndex++;
            await UpdateStores();
        }

        private void ExecuteSelectStoreCommand(object obj)
        {
            if (SelectedStore.Store.StoreId.HasValue)
            {
                RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new OrdersViewModel(_accessToken, SelectedStore.Store.StoreId.Value)));
            }
        }

        private async void ExecuteSearchCommand(object obj)
        {
            _pageIndex = 1;
            await UpdateStores();
        }

        public Task NavigateFrom()
        {
            return Task.CompletedTask;
        }

        public Task NavigateTo()
        {
            return Task.CompletedTask;
        }

        private async Task UpdateStores()
        {
            try
            {
                var stores = await GetStoresAsync(_pageIndex);

                Stores.Clear();

                foreach (var store in stores)
                {
                    Stores.Add(new StoreViewModel(store));
                }
            }
            catch
            {
                // ignored
            }
        }

        private async Task<IEnumerable<Store>> GetStoresAsync(int page)
        {
            var storesResponse = await _storesApi.GetStoresAsync(SearchQuery, page, StoresPerPage).ConfigureAwait(false);

            if (storesResponse.TotalRecordCount.HasValue)
            {
                var totalRecords = storesResponse.TotalRecordCount.Value;
                _totalPages = totalRecords / StoresPerPage + (totalRecords % StoresPerPage > 0 ? 1 : 0);
            }

            return storesResponse.Data;
        }
    }
}
