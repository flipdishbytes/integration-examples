using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;

namespace WpfIntegration
{
    /// <summary>
    /// Interaction logic for LogoutWebView.xaml
    /// </summary>
    public partial class LogoutWebView : Window
    {
        public event EventHandler Done;
        private bool _navigatedToCallBackAlready;

        Uri _callbackUri;

        public LogoutWebView()
        {
            InitializeComponent();
            webView.Navigated += WebView_Navigated;
        }

        private void WebView_Navigated(object sender, NavigationEventArgs e)
        {
            var navigateToCallbackUri = e.Uri.ToString().StartsWith(_callbackUri.AbsoluteUri);

            if (!navigateToCallbackUri)
                return;

            if (_navigatedToCallBackAlready)
            {
                Hide();
                RaiseDone();
            }

            _navigatedToCallBackAlready = true;
        }

        public void Start(Uri startUri, Uri callbackUri)
        {
            _navigatedToCallBackAlready = false;
            _callbackUri = callbackUri;
            webView.Navigate(startUri);
        }
        
        private void RaiseDone()
        {
            Done?.Invoke(this, EventArgs.Empty);
        }
    }
}
