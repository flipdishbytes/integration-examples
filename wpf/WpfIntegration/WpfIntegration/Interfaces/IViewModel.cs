using System;
using System.Threading.Tasks;
using WpfIntegration.Infrastructure;

namespace WpfIntegration.Interfaces
{
    public interface IViewModel
    {
        event EventHandler<AppNavigationEventArgs> RequestNavigation;

        Task NavigateFrom();
        Task NavigateTo();
    }
}
