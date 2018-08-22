using System;
using System.Configuration;

namespace WpfIntegration.Infrastructure
{
    /// <summary>
    /// This is a class used for reading the properties
    /// </summary>
    class AppSettings
    {
        private static AppSettings _settings;

        public static AppSettings Settings
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

        public string ClientId => ConfigurationManager.AppSettings["ClientId"] ?? throw new ArgumentException(nameof(ClientId));

        public bool SaveOrdersToFile
        {
            get
            {
                var settingValue = ConfigurationManager.AppSettings["SaveOrdersToFile"];
                if (string.IsNullOrEmpty(settingValue))
                    return false;

                return bool.TryParse(settingValue, out bool result) && result;
            }
        }
    }
}
