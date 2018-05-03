using System;
using WpfIntegration.Interfaces;

namespace WpfIntegration.Infrastructure
{
    /// <summary>
    /// This is used for the navigation throughout the application
    /// </summary>
    public class AppNavigationEventArgs : EventArgs
    {
        /// <summary>
        /// This is used for the navigation throughout the application
        /// </summary>
        /// <param name="navigateTo">Registered View Model (can be registered in MainWindow.xaml.cs) for Navigation</param>
        public AppNavigationEventArgs(IViewModel navigateTo)
        {
            ViewModel = navigateTo;
        }

        public IViewModel ViewModel { get; }
    }
}
