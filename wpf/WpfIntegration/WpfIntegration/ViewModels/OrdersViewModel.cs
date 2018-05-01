using System;
using System.Threading.Tasks;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;

namespace WpfIntegration.ViewModels
{
    class OrdersViewModel : BindableBase, IViewModel
    {
        public event EventHandler<AppNavigationEventArgs> RequestNavigation;

        public Task NavigateFrom()
        {
            throw new NotImplementedException();
        }

        public Task NavigateTo()
        {
            throw new NotImplementedException();
        }
    }
}
