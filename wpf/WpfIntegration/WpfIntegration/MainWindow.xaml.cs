using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;
using WpfIntegration.ViewModels;
using WpfIntegration.Views;

namespace WpfIntegration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //This is used to hold our registered navigation pairs
        private readonly Dictionary<Type, Grid> _viewViewModelPairs = new Dictionary<Type, Grid>();
        
        public MainWindow()
        {
            InitializeComponent();

            //Register basic pairs for navigation
            _viewViewModelPairs.Add(typeof(LoginViewModel), new LoginView());
            _viewViewModelPairs.Add(typeof(OrdersViewModel), new OrdersView());
            _viewViewModelPairs.Add(typeof(OrderReadyToProccessViewModel), new OrderReadyToProccessView());
            _viewViewModelPairs.Add(typeof(StoresViewModel), new StoresView());

            //Navigate to the first view model, from here on navigation will be handled in the view models
            NavigateTo(this, new AppNavigationEventArgs(new LoginViewModel()));
        }

        /// <summary>
        /// Basic navigation implementation, you should only call it from a class that inherits from IViewModel
        /// </summary>
        /// <param name="sender">View model that triggered this event</param>
        /// <param name="navigationArgs">Arguments passed for navigation</param>
        private async void NavigateTo(object sender, AppNavigationEventArgs navigationArgs)
        {
            try
            {
                //Get the type of the view model
                var navigationType = navigationArgs.ViewModel.GetType();

                //Search for the type in the registered pairs
                if (!_viewViewModelPairs.ContainsKey(navigationType))
                {
                    throw new ArgumentOutOfRangeException(nameof(navigationArgs.ViewModel));
                }

                //If the triggering, make sure you call navigate from & unsubscribe from the event subscribed to below
                if (sender is IViewModel currentViewModel)
                {
                    await currentViewModel.NavigateFrom();
                    currentViewModel.RequestNavigation -= NavigateTo;
                }

                //Remove any & all children from our NavigationGrid that we use for navigation
                NavigationGrid.Children.Clear();

                //Get an existing view from the registered view - view model pairs
                var view = _viewViewModelPairs[navigationType];

                //Set the data context of the view to the view model
                view.DataContext = navigationArgs.ViewModel;

                //Subscribe to the navigated event, this will be called when the view model wants to navigate to the next page
                navigationArgs.ViewModel.RequestNavigation += NavigateTo;

                //Add the view that we recieved as a child of the NavigationGrid
                NavigationGrid.Children.Add(view);

                //Call the navigate to, in order to initialize the view model
                await navigationArgs.ViewModel.NavigateTo();
            }
            catch(Exception e)
            {
                Console.WriteLine($"Unhandled exception occured: {e.Message}");
            }
        }

        /// <summary>
        /// We are required to close the WebView in case we close the app on the screen that creates the WebView window
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (NavigationGrid != null && NavigationGrid.Children.Count > 0)
            {
                foreach (var navigationGridChild in NavigationGrid.Children)
                {
                    if (!(navigationGridChild is Grid grid)) continue;

                    if (grid.DataContext != null && grid.DataContext is IViewModel viewModel)
                    {
                        viewModel.NavigateFrom();
                    }
                }
            }
            base.OnClosing(e);
        }
    }
}
