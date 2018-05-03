using Flipdish.Api;
using Flipdish.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;

namespace WpfIntegration.ViewModels
{
    class StoresViewModel : BindableBase, IViewModel
    {
        private const int StoresPerPage = 15;

        public event EventHandler<AppNavigationEventArgs> RequestNavigation;

        private readonly StoresApi _storesApi;

        private int _pageIndex = 1;
        private int _totalPages;
        private StoreViewModel _selectedStore;
        private string _searchQuery;

        public StoresViewModel()
        {
            //Create the stores api
            _storesApi = new StoresApi();

            Stores = new ObservableCollection<StoreViewModel>();
            PreviousPageCommand = new RelayCommand(ExecutePreviousPageCommand, m => _pageIndex > 1);
            NextPageCommand = new RelayCommand(ExecuteNextPageCommand, m => _pageIndex < _totalPages); 
            SelectStoreCommand = new RelayCommand(ExecuteSelectStoreCommand, m => SelectedStore != null);
            SearchCommand = new RelayCommand(ExecuteSearchCommand);
            LogoutCommand = new RelayCommand(ExecuteLogoutCommand);
        }


        public ObservableCollection<StoreViewModel> Stores { get; }
        public StoreViewModel SelectedStore
        {
            get => _selectedStore;
            set => SetProperty(ref _selectedStore, value);
        }
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand SelectStoreCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand LogoutCommand { get; }

        /// <summary>
        /// Go to previous page of stores
        /// Can execute checks that the index will not fall out of bounds
        /// </summary>
        /// <param name="obj">Nothing</param>
        private async void ExecutePreviousPageCommand(object obj)
        {
            _pageIndex--;
            await UpdateStores();
        }

        /// <summary>
        /// Go to next page of stores
        /// Can execute checks that the index will not fall out of bounds
        /// </summary>
        /// <param name="obj">Nothing</param>
        private async void ExecuteNextPageCommand(object obj)
        {
            _pageIndex++;
            await UpdateStores();
        }

        /// <summary>
        /// Execute a search command and set the page index to 1
        /// When we are searching we want to make sure to be on the first page of the search
        /// </summary>
        /// <param name="obj">Nothing</param>
        private async void ExecuteSearchCommand(object obj)
        {
            _pageIndex = 1;
            await UpdateStores();
        }

        /// <summary>
        /// Execute select store command, If StoreId is not null then we navigate to the Orders View.
        /// The Can Execute checks if SelectedStore is not null.
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteSelectStoreCommand(object obj)
        {
            if (SelectedStore.Store.StoreId.HasValue)
            {
                RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new OrdersViewModel(SelectedStore.Store.StoreId.Value)));
            }
        }

        /// <summary>
        /// Logout from the system
        /// </summary>
        /// <param name="obj">Nothing</param>
        private void ExecuteLogoutCommand(object obj)
        {
            OauthService.Service.Logout();
        }

        /// <summary>
        /// This is used to update the list of stores when we Paginate & Search
        /// </summary>
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

        /// <summary>
        /// Occurs when logout is succsesful
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OauthServiceOnLogoutDone(object sender, EventArgs e)
        {
            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new LoginViewModel()));
        }

        /// <summary>
        /// This is used to retrieve the paginated list of stores from the backend
        /// </summary>
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

        public Task NavigateFrom()
        {
            OauthService.Service.LogoutDone -= OauthServiceOnLogoutDone;
            return Task.CompletedTask;
        }

        public Task NavigateTo()
        {
            OauthService.Service.LogoutDone += OauthServiceOnLogoutDone;
            return Task.CompletedTask;
        }
    }
}
