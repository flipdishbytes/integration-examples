using Flipdish.Model;
using System;
using System.Windows.Input;
using WpfIntegration.Infrastructure;

namespace WpfIntegration.ViewModels
{
    /// <summary>
    /// Single order view model, used for displaying a single order line
    /// </summary>
    class OrderViewModel : BindableBase
    {
        public event EventHandler<Order> OrderViewRequested;

        private Order _order;

        public OrderViewModel(Order o)
        {
            Order = o;
            ViewOrderCommand = new RelayCommand(ExecuteViewOrderCommand);
        }
        
        public Order Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        public ICommand ViewOrderCommand { get; }

        private void ExecuteViewOrderCommand(object obj)
        {
            OrderViewRequested?.Invoke(this, _order);
        }
    }
}
