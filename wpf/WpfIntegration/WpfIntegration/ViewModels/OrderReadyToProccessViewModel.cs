using Flipdish.Api;
using Flipdish.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;

namespace WpfIntegration.ViewModels
{
    class OrderReadyToProccessViewModel : BindableBase, IViewModel
    {
        public event EventHandler<AppNavigationEventArgs> RequestNavigation;
        
        private readonly int _storeId;
        private readonly Order _order;
        private readonly OrdersApi _ordersApi;

        private int _estimatedMinutesForDeliveryIndex;
        private Reject.RejectReasonEnum _rejectReason;

        public OrderReadyToProccessViewModel(int storeId, Order order)
        {
            _storeId = storeId;
            _order = order;
            _ordersApi = new OrdersApi();

            OrderSummary = BuildOrderSummary();
            RejectValues = Enum.GetValues(typeof(Reject.RejectReasonEnum));
            EstimatedMinutesForDeliveryValues = new List<int>
            {
                30,
                45,
                60,
                75
            };
            
            EstimatedMinutesForDeliveryIndex = 0;
            RejectReason = Reject.RejectReasonEnum.TooBusy;

            AcceptCommand = new RelayCommand(ExecuteAcceptCommand);
            DeclineCommand = new RelayCommand(ExecuteDeclineCommand);
        }
        
        public string OrderSummary { get; }
        
        public List<int> EstimatedMinutesForDeliveryValues { get; }
        public int EstimatedMinutesForDeliveryIndex
        {
            get => _estimatedMinutesForDeliveryIndex;
            set => SetProperty(ref _estimatedMinutesForDeliveryIndex, value);
        }

        public Array RejectValues { get; }
        public Reject.RejectReasonEnum RejectReason
        {
            get => _rejectReason;
            set => SetProperty(ref _rejectReason, value);
        }

        public ICommand AcceptCommand { get; }
        public ICommand DeclineCommand { get; }

        /// <summary>
        /// This is used to accept the order with an estimated time for delivery
        /// </summary>
        private async void ExecuteAcceptCommand(object obj)
        {
            await _ordersApi.AcceptOrderAsync(_order.OrderId, new Accept(EstimatedMinutesForDeliveryValues[EstimatedMinutesForDeliveryIndex]));
            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new OrdersViewModel(_storeId)));
        }

        /// <summary>
        /// This is used to reject the order with a reject reason
        /// </summary>
        private async void ExecuteDeclineCommand(object obj)
        {
            await _ordersApi.RejectOrderAsync(_order.OrderId, new Reject(RejectReason));
            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new OrdersViewModel(_storeId)));
        }
        
        /// <summary>
        /// This creates the order summary with some basic information
        /// </summary>
        /// <returns>Order summary</returns>
        private string BuildOrderSummary()
        {
            if (_order == null) return string.Empty;

            var sb = new StringBuilder();
            sb.Append("Customer Name: ").AppendLine(_order.Customer.Name);
            sb.Append("Customer EmailAddress: ").AppendLine(_order.Customer.EmailAddress);
            sb.Append("Customer PhoneNumber: ").AppendLine(_order.Customer.PhoneNumber);
            sb.Append("Delivery Type:").AppendLine(_order.DeliveryType?.ToString() ?? string.Empty);
            sb.Append("Order Id: ").AppendLine(_order.OrderId.ToString());
            sb.Append("Order Total: ").AppendLine(_order.Amount.ToString());
            sb.Append("Chef's Notes: ").AppendLine(_order.ChefNote);
            sb.AppendLine("Order Items:");

            foreach (var item in _order.OrderItems)
            {
                sb.Append(" #").Append("Item Name: ").AppendLine(item.Name);
                foreach (var option in item.OrderItemOptions)
                {
                    sb.Append("   *").Append("Option: ").AppendLine(option.Name);
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
