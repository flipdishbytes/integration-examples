using IdentityModel;
using IdentityModel.Client;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using WpfIntegration.Infrastructure;
using WpfIntegration.Interfaces;

namespace WpfIntegration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private LoginWebView _login;
        private AuthorizeResponse _response;

        public event PropertyChangedEventHandler PropertyChanged;

        private IViewModel _currentViewModel;

        public IViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(value, ref _currentViewModel);
        }

        public MainWindow()
        {
            InitializeComponent();

            _login = new LoginWebView();
            _login.Done += (s, e) =>
            {
                _response = e;
            };

            Loaded += (s, e) =>
            {
                _login.Owner = this;
            };
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
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
        
        protected void SetProperty(object value, ref object property, [CallerMemberName]string caller = null)
        {
            if (property == value)
                return;

            property = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
