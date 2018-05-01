using System;
using WpfIntegration.Interfaces;

namespace WpfIntegration.Infrastructure
{
    public class AppNavigationEventArgs : EventArgs
    {
        public AppNavigationEventArgs(IViewModel navigateTo)
        {
            ViewModel = navigateTo;
        }

        public IViewModel ViewModel { get; }
    }
}
