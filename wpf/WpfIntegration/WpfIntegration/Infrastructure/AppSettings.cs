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

        public string ClientId => ConfigurationManager.AppSettings["ClientId"] ?? throw new ArgumentException(nameof(ClientId));

        public int PhysicalRestaurantId => int.Parse(ConfigurationManager.AppSettings["PhysicalRestaurantId"]);
    }
}
