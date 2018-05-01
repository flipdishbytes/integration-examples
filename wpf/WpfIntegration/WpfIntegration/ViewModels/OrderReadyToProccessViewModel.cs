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

        private readonly Order _order;
        private readonly OrdersApi _ordersApi;

        public string OrderSummary { get; }

        public ICommand AcceptCommand { get; }
        public ICommand DeclineCommand { get; }

        public OrderReadyToProccessViewModel(string accessToken, Order order)
        {
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

        private void ExecuteAcceptCommand(object obj)
        {
            throw new NotImplementedException();
        }

        private void ExecuteDeclineCommand(object obj)
        {
            throw new NotImplementedException();
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
