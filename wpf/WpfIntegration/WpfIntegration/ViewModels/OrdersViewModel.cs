using Flipdish.Api;
using Flipdish.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;

namespace WpfIntegration.ViewModels
{
    class OrdersViewModel : BindableBase, IViewModel
    {
        public event EventHandler<AppNavigationEventArgs> RequestNavigation;
        
        private readonly int _physicalStoreId;
        private readonly OrdersApi _ordersApi;
        
        private int _pageIndex = 1;
        private int _totalPages = 2;
        private const int OrdersPerPage = 17;
        private IDisposable _intervalObservable;

        public OrdersViewModel(int physicalStoreId)
        {
            _ordersApi = new OrdersApi();
            _physicalStoreId = physicalStoreId;
            Orders = new ObservableCollection<OrderViewModel>();
            NewOrders = new ObservableCollection<OrderViewModel>();

            PreviousPageCommand = new RelayCommand(ExecutePreviousPageCommand, m => _pageIndex > 1);
            NextPageCommand = new RelayCommand(ExecuteNextPageCommand, m => _pageIndex < _totalPages);
            LogoutCommand = new RelayCommand(ExecuteLogoutCommand);
            BackCommand = new RelayCommand(ExecuteBackButton);
        }
        
        public ObservableCollection<OrderViewModel> Orders { get; }
        public ObservableCollection<OrderViewModel> NewOrders { get; }

        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand BackCommand { get; }

        /// <summary>
        /// Go to previous page of stores
        /// Can execute checks that the index will not fall out of bounds
        /// </summary>
        /// <param name="obj">Nothing</param>
        private async void ExecutePreviousPageCommand(object obj)
        {
            _pageIndex--;
            await LoadOrders();
        }

        /// <summary>
        /// Go to next page of stores
        /// Can execute checks that the index will not fall out of bounds
        /// </summary>
        /// <param name="obj">Nothing</param>
        private async void ExecuteNextPageCommand(object obj)
        {
            _pageIndex++;
            await LoadOrders();
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
        /// Navigates back to store selection
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteBackButton(object obj)
        {
            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new StoresViewModel()));
        }

        /// <summary>
        /// This is used to update orders that are ready to process by the restaurant
        /// </summary>
        /// <returns></returns>
        private async Task UpdateReadyToProcessOrders()
        {
            try
            {
                var readyOrders = await GetReadyOrdersAsync();

                readyOrders = readyOrders.Where(r => NewOrders.All(o => o.Order.OrderId != r.OrderId));

                foreach (var order in readyOrders)
                {
                    var newOrder = new OrderViewModel(order);
                    newOrder.OrderViewRequested += NewOrder_OrderViewRequested;
                    NewOrders.Add(newOrder);
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
        /// This is used to navigate to the Order Processing View
        /// </summary>
        private void NewOrder_OrderViewRequested(object sender, Order e)
        {
            //Get the view model associated with this order
            var orderViewModel = NewOrders.FirstOrDefault(m => m.Order.OrderId == e.OrderId);

            //Unsubscribe from the event & remove the view model from new orders
            if (orderViewModel != null)
            {
                orderViewModel.OrderViewRequested -= NewOrder_OrderViewRequested;
                NewOrders.Remove(orderViewModel);
            }

            //Navigate to the Order Ready to Process and pass this Order
            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new OrderReadyToProccessViewModel(_physicalStoreId, e)));
        }

        /// <summary>
        /// This is used to update the list of stores when we Paginate & Load the page first time
        /// </summary>
        private async Task LoadOrders()
        {
            try
            {
                foreach (var orderViewModel in Orders)
                {
                    orderViewModel.OrderViewRequested -= Order_OrderViewRequested;
                }

                Orders.Clear();

                var orders = await GetOrdersAsync(_pageIndex);

                foreach (var order in orders)
                {
                    var orderViewModel = new OrderViewModel(order);
                    orderViewModel.OrderViewRequested += Order_OrderViewRequested;
                    Orders.Add(orderViewModel);

                    TrySaveNewAcceptedOrdersToFile(order);
                }
            }
            catch
            {
                // ignored
            }
        }


        private void TrySaveNewAcceptedOrdersToFile(Order order)
        {
            if(!AppSettings.Settings.SaveOrdersToFile)
            {
                return;
            }

            var fileName = order.OrderId + ".json";
            if (File.Exists(fileName))
            {
                return;
            }
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(order);
            File.WriteAllText(fileName, json);
        }

        private void Order_OrderViewRequested(object sender, Order e)
        {
            if (sender is IViewModel viewModel)
            {
                RequestNavigation?.Invoke(this, new AppNavigationEventArgs(viewModel));
            }
        }

        /// <summary>
        /// This is used to retrieve the paginated list of orders from the backend
        /// </summary>
        private async Task<IEnumerable<Order>> GetOrdersAsync(int page)
        {
            var restaurants = new List<int?> { _physicalStoreId };
            var ordersResponse = await _ordersApi.GetOrdersAsync(restaurants, null, page, OrdersPerPage).ConfigureAwait(false);

            if (ordersResponse.TotalRecordCount.HasValue)
            {
                var totalRecords = ordersResponse.TotalRecordCount.Value;
                _totalPages = totalRecords / OrdersPerPage + (totalRecords % OrdersPerPage > 0 ? 1 : 0);
            }

            return ordersResponse.Data.Where(o => o.OrderState != Order.OrderStateEnum.ReadyToProcess);
        }

        /// <summary>
        /// This is used to retrieve the Ready To Process list of orders from the backend
        /// </summary>
        private async Task<IEnumerable<Order>> GetReadyOrdersAsync()
        {
            var restaurants = new List<int?> { _physicalStoreId };
            var ordersResponse = await _ordersApi.GetOrdersAsync(restaurants, new List<string> { "ReadyToProcess" }).ConfigureAwait(false);
            return ordersResponse.Data;
        }

        public Task NavigateFrom()
        {
            //We dispose the observable that we create when we navigate to this page, to make sure that it doesn't run in the background
            _intervalObservable.Dispose();
            OauthService.Service.LogoutDone -= OauthServiceOnLogoutDone;
            return Task.CompletedTask;
        }
        
        public async Task NavigateTo()
        {
            OauthService.Service.LogoutDone += OauthServiceOnLogoutDone;

            //This piece of code runs every 5 seconds, it checks if any orders are ready to be processed by the restaurant
            //It executes OnNextInterval every 5 seconds and subscribes & observes on current thread
            _intervalObservable = Observable.Interval(TimeSpan.FromSeconds(5))
                .SubscribeOn(Scheduler.CurrentThread)
                .ObserveOn(DispatcherScheduler.Current)
                .Subscribe(async (i) => await UpdateReadyToProcessOrders());

            //Get all orders
            await LoadOrders();
        }
    }
}
