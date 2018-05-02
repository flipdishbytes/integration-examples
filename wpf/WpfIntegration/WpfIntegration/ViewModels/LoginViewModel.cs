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

        private readonly LoginWebView _login;

        public LoginViewModel()
        {
            _login = new LoginWebView();
            LoginCommand = new RelayCommand(ExecuteLoginCommand);
        }

        public ICommand LoginCommand { get; }

        private void ExecuteLoginCommand(object obj)
        {
            RequestToken("openid api", OidcConstants.ResponseTypes.CodeIdTokenToken);
        }
        
        private void RequestToken(string scope, string responseType)
        {
            var redirectUri = "oob://localhost/wpf.webview.client";

            var request = new RequestUrl($"{AppSettings.Settings.Endpoint}identity/connect/authorize");

            var startUrl = request.CreateAuthorizeUrl(
                clientId: AppSettings.Settings.ClientId,
                responseType: responseType,
                scope: scope,
                redirectUri: redirectUri,
                nonce: CryptoRandom.CreateUniqueId());

            _login.Show();
            _login.Start(new Uri(startUrl), new Uri(redirectUri));
        }

        private void _login_Done(object sender, AuthorizeResponse e)
        {
            //Set default API configuration base path
            Configuration.Default.BasePath = AppSettings.Settings.Endpoint;
            //Add default API configuration header (this header is required to work with the API)
            Configuration.Default.DefaultHeader.Add("Authorization", $"Bearer {e.AccessToken}");

            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new StoresViewModel()));
        }

        public Task NavigateFrom()
        {
            _login.Done -= _login_Done;
            return Task.CompletedTask;
        }

        public Task NavigateTo()
        {
            _login.Done += _login_Done;
            return Task.CompletedTask;
        }
    }
}
