using Flipdish.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Flipdish.Api;
using Flipdish.Client;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;

namespace WpfIntegration.ViewModels
{
    class OrderReadyToProccessViewModel : BindableBase, IViewModel
    {
        public event EventHandler<AppNavigationEventArgs> RequestNavigation;

        private readonly string _accessToken;
        private readonly Order _order;
        private readonly OrdersApi _ordersApi;
        private int _estimatedMinutesForDelivery;
        private Reject.RejectReasonEnum _rejectReason;
        
        public OrderReadyToProccessViewModel(string accessToken, Order order)
        {
            _accessToken = accessToken;
            _order = order;
            OrderSummary = BuildOrderSummary();
            AcceptCommand = new RelayCommand(ExecuteAcceptCommand);
            DeclineCommand = new RelayCommand(ExecuteDeclineCommand);

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

        public string OrderSummary { get; }

        public int EstimatedMinutesForDelivery
        {
            get => _estimatedMinutesForDelivery;
            set => SetProperty(ref _estimatedMinutesForDelivery, value);
        }

        public Reject.RejectReasonEnum RejectReason
        {
            get => _rejectReason;
            set => SetProperty(ref _rejectReason, value);
        }

        public ICommand AcceptCommand { get; }
        public ICommand DeclineCommand { get; }

        private async void ExecuteAcceptCommand(object obj)
        {
            await _ordersApi.AcceptOrderAsync(_order.OrderId, new Accept(EstimatedMinutesForDelivery));
            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new OrdersViewModel(_accessToken)));
        }

        private async void ExecuteDeclineCommand(object obj)
        {
            await _ordersApi.RejectOrderAsync(_order.OrderId, new Reject(RejectReason));
            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new OrdersViewModel(_accessToken)));
        }

        private string BuildOrderSummary()
        {
            if (_order == null) return string.Empty;

            var sb = new StringBuilder();
            sb.Append("Customer Name: ").AppendLine(_order.Customer.Name);
            sb.Append("Customer Name: ").AppendLine(_order.Customer.EmailAddress);
            sb.Append("Customer Name: ").AppendLine(_order.Customer.PhoneNumber);
            sb.Append("Customer Location:").Append(_order.CustomerLocation.Latitude).Append(".").AppendLine(_order.CustomerLocation.Longitude.ToString());
            sb.Append("Order Id: ").AppendLine(_order.OrderId.ToString());
            sb.Append("Order Total: ").AppendLine(_order.Amount.ToString());
            sb.Append("Chef's Notes: ").AppendLine(_order.ChefNote);
            sb.AppendLine("Order Items:");

            foreach (var item in _order.OrderItems)
            {
                sb.Append("   ").Append("Item Name: ").Append(item.Name);
                foreach (var option in item.OrderItemOptions)
                {
                    sb.Append("      ").Append("Option: ").Append(option.Name);
                }
            }

            return sb.ToString();

        }

        public Task NavigateFrom()
        {
            return Task.CompletedTask;
        }

        public Task NavigateTo()
        {
            return Task.CompletedTask;
        }
    }
}
