using Flipdish.Model;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;

namespace WpfIntegration.ViewModels
{
    /// <summary>
    /// Single order view model, used for displaying a single order line
    /// </summary>
    class OrderViewModel : BindableBase, IViewModel
    {
        public event EventHandler<Order> OrderViewRequested;

        private Order _order;

        public OrderViewModel(Order o)
        {
            Order = o;
            OrderSummary = BuildOrderSummary();
            ViewOrderCommand = new RelayCommand(ExecuteViewOrderCommand);
            BackCommand = new RelayCommand(ExecuteBackButton);
        }
        
        public Order Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        public string OrderSummary { get; }

        public ICommand ViewOrderCommand { get; }
        public ICommand BackCommand { get; }

        private void ExecuteViewOrderCommand(object obj)
        {
            OrderViewRequested?.Invoke(this, _order);
        }

        /// <summary>
        /// Navigates back to store selection
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteBackButton(object obj)
        {
            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new OrdersViewModel(_order.Store.Id.Value)));
        }

        /// <summary>
        /// This creates the order summary with some basic information
        /// </summary>
        /// <returns>Order summary</returns>
        private string BuildOrderSummary()
        {
            if (_order == null) return string.Empty;

            var sb = new StringBuilder();
            sb.Append("Flipdish >> ").Append(_order.OrderId?.ToString() ?? string.Empty).Append(" >> ").AppendLine(_order.Store?.Name ?? string.Empty);
            sb.Append("Time of order: ").AppendLine(_order.PlacedTime?.ToString() ?? string.Empty);
            sb.Append("Delivery Type:").AppendLine(_order.DeliveryType?.ToString() ?? string.Empty);
            sb.Append("Chef's Notes: ").AppendLine(_order.ChefNote ?? string.Empty).AppendLine();

            sb.AppendLine("Order Items:");
            foreach (var item in _order.OrderItems)
            {
                sb.Append(" #").Append("Item Name: ").Append(item.Name ?? string.Empty).Append("...").AppendLine(item.Price?.ToString() ?? string.Empty);
                foreach (var option in item.OrderItemOptions)
                {
                    sb.Append("   -").Append("Option: ").AppendLine(option.Name ?? string.Empty);
                }
            }

            sb.AppendLine().Append("Processing Fee: ").AppendLine(_order.ProcessingFee?.ToString() ?? string.Empty);
            sb.Append("Order Total: ").AppendLine(_order.Amount?.ToString() ?? string.Empty);
            sb.Append("Payment Type: ").AppendLine(_order.PaymentAccountType?.ToString() ?? string.Empty).AppendLine();

            sb.Append("Customer Info:").AppendLine();
            sb.Append(" - Name: ").AppendLine(_order.Customer?.Name ?? string.Empty);
            sb.Append(" - Phone Number: ").AppendLine(_order.Customer?.PhoneNumber ?? string.Empty);
            sb.Append(" - Email Address: ").AppendLine(_order.Customer?.EmailAddress ?? string.Empty);

            return sb.ToString();
        }

        public event EventHandler<AppNavigationEventArgs> RequestNavigation;

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
