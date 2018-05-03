using IdentityModel;
using IdentityModel.Client;
using System;

namespace WpfIntegration.Infrastructure
{
    class OauthService
    {
        private static OauthService _service;

        public static OauthService Service
        {
            get
            {
                if (_service != null)
                    return _service;

                return _service = new OauthService();
            }
        }

        private LoginWebView _login;
        private LogoutWebView _logout;
        private AuthorizeResponse _authorizeResponse;

        private OauthService() { }

        public event EventHandler<AuthorizeResponse> LoginDone;
        public event EventHandler LogoutDone;

        /// <summary>
        /// Creates the authorization request and shows the popup with the Web View.
        /// When the login is succesfully complete triggers you the LoginDone event.
        /// </summary>
        public void Login(string scope, string responseType)
        {
            const string redirectUri = "oob://localhost/wpf.webview.client";

            var request = new RequestUrl($"{AppSettings.Settings.Endpoint}identity/connect/authorize");

            var startUrl = request.CreateAuthorizeUrl(
                clientId: AppSettings.Settings.ClientId,
                responseType: responseType,
                scope: scope,
                redirectUri: redirectUri,
                nonce: CryptoRandom.CreateUniqueId());

            _login = new LoginWebView();
            _login.Done += _login_Done;
            _login.Show();
            _login.Start(new Uri(startUrl), new Uri(redirectUri));

        }

        private void _login_Done(object sender, AuthorizeResponse e)
        {
            _authorizeResponse = e;
            _login.Done -= _login_Done;
            _login.Close();

            LoginDone?.Invoke(sender, e);
        }

        /// <summary>
        /// Creates the end session request and shows the popup with the Web View.
        /// When the logout is succesfully complete triggers the LogoutDone event.
        /// </summary>
        public void Logout()
        {
            _logout = new LogoutWebView();
            _logout.Done += _logout_Done;
            _logout.Show();
            _logout.Start(new Uri($"{AppSettings.Settings.Endpoint}identity/connect/endsession"), new Uri("https://localhost/identity/logout"));
        }

        private void _logout_Done(object sender, EventArgs e)
        {
            _authorizeResponse = null;
            _logout.Done -= _logout_Done;
            _logout.Close();

            LogoutDone?.Invoke(sender, e);
        }
    }
}
