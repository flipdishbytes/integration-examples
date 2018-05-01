using IdentityModel;
using IdentityModel.Client;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;

namespace WpfIntegration.ViewModels
{
    class LoginViewModel : BindableBase, IViewModel
    {
        public event EventHandler<AppNavigationEventArgs> RequestNavigation;

        private LoginWebView _login;
        private AuthorizeResponse _response;

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
            var request = new RequestUrl($"{AppSettings.Settings.Endpoint}identity/connect/authorize");
            var startUrl = request.CreateAuthorizeUrl(
                clientId: AppSettings.Settings.OAuthClientId,
                responseType: responseType,
                scope: scope,
                redirectUri: "oob://localhost/wpf.webview.client",
                nonce: CryptoRandom.CreateUniqueId());

            _login.Show();
            _login.Start(new Uri(startUrl), new Uri("oob://localhost/wpf.webview.client"));
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

        private void _login_Done(object sender, AuthorizeResponse e)
        {
            _response = e;
            RequestNavigation?.Invoke(this, new AppNavigationEventArgs(new OrdersViewModel()));
        }
    }
}
