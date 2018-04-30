using System;
using System.Configuration;
using WpfIntegration.Interfaces;

namespace WpfIntegration.Infrastructure
{
    class AppSettings : IAppSettings
    {
        private static IAppSettings _settings;

        public static IAppSettings Settings
        {
            get
            {
                if (_settings != null)
                    return _settings;

                return _settings = new AppSettings();
            }
        }

        private AppSettings() { }

        public string Endpoint => ConfigurationManager.AppSettings["Endpoint"] ?? throw new ArgumentException(nameof(Endpoint));

        public string OAuthClientId => ConfigurationManager.AppSettings["OAuthClientId"] ?? throw new ArgumentException(nameof(OAuthClientId));

        public string OAuthSecretKey => ConfigurationManager.AppSettings["OAuthSecretKey"] ?? throw new ArgumentException(nameof(OAuthSecretKey));
    }
}
