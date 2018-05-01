using Flipdish.Api;
using Flipdish.Client;
using Flipdish.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;
using System.Reactive.Concurrency;

namespace WpfIntegration.ViewModels
{
    class OrdersViewModel : BindableBase, IViewModel
    {
        private readonly string _accessToken;
        public event EventHandler<AppNavigationEventArgs> RequestNavigation;

        private readonly OrdersApi _ordersApi;
        private IDisposable _intervalObservable;

        public OrdersViewModel(string accessToken)
        {
            _accessToken = accessToken;
            Orders = new ObservableCollection<OrderViewModel>();
            NewOrders = new ObservableCollection<OrderViewModel>();

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
            _ordersApi = new OrdersApi(configuration);
        }

        public ObservableCollection<OrderViewModel> Orders { get; }
        public ObservableCollection<OrderViewModel> NewOrders { get; }

        public Task NavigateFrom()
        {
            //We dispose the observable that we create when we navigate to this page, to make sure that it doesn't run in the background
            _intervalObservable.Dispose();

            return Task.CompletedTask;
        }
        public async Task NavigateTo()
        {
            //Get all orders
            var orders = await GetOrdersAsync(1, 200);

            foreach (var order in orders)
            {
                Orders.Add(new OrderViewModel(order));
            }

            //This piece of code runs every 5 seconds, it checks if any orders are ready to be processed by the restaurant
            _intervalObservable = Observable.Interval(TimeSpan.FromSeconds(5)).SubscribeOn(Scheduler.CurrentThread).ObserveOn(DispatcherScheduler.Current).Subscribe(async i => 
            {
                var readyOrders = await GetReadyOrdersAsync();

                readyOrders = readyOrders.Where(r => NewOrders.All(o => o.Order.OrderId != r.OrderId));

                foreach (var order in readyOrders)
                {
                    var newOrder = new OrderViewModel(order);
                    newOrder.OrderViewRequested += NewOrder_OrderViewRequested;
                    NewOrders.Add(newOrder);
                }
            });
        }

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
            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new OrderReadyToProccessViewModel(_accessToken, e)));
        }

        private async Task<IEnumerable<Order>> GetOrdersAsync(int page, int limit)
        {
            var restaurants = new List<int?> { AppSettings.Settings.PhysicalRestaurantId };
            var ordersResponse = await _ordersApi.GetOrdersAsync(restaurants, null, page, limit).ConfigureAwait(false);
            return ordersResponse.Data.Where(o => o.OrderState != Order.OrderStateEnum.ReadyToProcess);
        }

        private async Task<IEnumerable<Order>> GetReadyOrdersAsync()
        {
            var restaurants = new List<int?> { AppSettings.Settings.PhysicalRestaurantId };
            var ordersResponse = await _ordersApi.GetOrdersAsync(restaurants, new List<string> { "ReadyToProcess" }).ConfigureAwait(false);
            return ordersResponse.Data;
        }
    }
}
