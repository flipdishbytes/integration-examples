using IdentityModel.Client;
using mshtml;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfIntegration
{
    /// <summary>
    /// Interaction logic for LoginWebView.xaml
    /// </summary>
    public partial class LoginWebView : Window
    {
        public event EventHandler<AuthorizeResponse> Done;

        Uri _callbackUri;

        public LoginWebView()
        {
            InitializeComponent();
            webView.Navigating += WebView_Navigating;

            Closing += (s, e) =>
            {
                Hide(e);
            };
        }

        public void Start(Uri startUri, Uri callbackUri)
        {
            _callbackUri = callbackUri;
            webView.Navigate(startUri);
        }

        private void Finish(string resultUrl, CancelEventArgs e)
        {
            Hide(e);
            RaiseDone(new AuthorizeResponse(resultUrl));
        }

        private void Hide(CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private void RaiseDone(AuthorizeResponse authorizeResponse)
        {
            if (Done == null)
                return;

            Done.Invoke(this, authorizeResponse);
        }

        private void WebView_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var navigateToCallbackUri = e.Uri.ToString().StartsWith(_callbackUri.AbsoluteUri);
            if (!navigateToCallbackUri)
                return;

            if (e.Uri.AbsoluteUri.Contains("#"))
            {
                Finish(e.Uri.AbsoluteUri, e);
                return;
            }

            var document = (IHTMLDocument3)((WebBrowser)sender).Document;
            var inputElements = document.getElementsByTagName("INPUT").OfType<IHTMLElement>();
            var resultUrl = "?";

            foreach (var input in inputElements)
            {
                resultUrl += input.getAttribute("name") + "=";
                resultUrl += input.getAttribute("value") + "&";
            }

            resultUrl = resultUrl.TrimEnd('&');
            Finish(resultUrl, e);
        }
    }
}
