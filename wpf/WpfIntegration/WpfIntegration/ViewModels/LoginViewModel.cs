using IdentityModel;
using IdentityModel.Client;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Flipdish.Client;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;

namespace WpfIntegration.ViewModels
{
    class LoginViewModel : BindableBase, IViewModel
    {
        public event EventHandler<AppNavigationEventArgs> RequestNavigation;
        
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLoginCommand);
        }

        public ICommand LoginCommand { get; }

        private void ExecuteLoginCommand(object obj)
        {
            OauthService.Service.Login("openid api", OidcConstants.ResponseTypes.CodeIdTokenToken);
        }

        /// <summary>
        /// Occurs when login page is done
        /// Sets basic Configuration for the Flipdish API and navigates to the Stores View
        /// </summary>
        private void _login_Done(object sender, AuthorizeResponse e)
        {
            //Set default API configuration base path
            Configuration.Default.BasePath = AppSettings.Settings.Endpoint;
            Configuration.Default.AccessToken = e.AccessToken;
            //In order to make the calls to the API we need to have a Bearer token associated with our request.
            //This creates or sets the bearer token required to get a reply from our API.
            if (Configuration.Default.DefaultHeader.ContainsKey("Authorization"))
            {
                Configuration.Default.DefaultHeader["Authorization"] = $"Bearer {e.AccessToken}";
            }
            else
            {
                Configuration.Default.DefaultHeader.Add("Authorization", $"Bearer {e.AccessToken}");
            }

            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new StoresViewModel()));
        }

        public Task NavigateFrom()
        {
            OauthService.Service.LoginDone -= _login_Done;
            return Task.CompletedTask;
        }

        public Task NavigateTo()
        {
            OauthService.Service.LoginDone += _login_Done;
            return Task.CompletedTask;
        }
    }
}
