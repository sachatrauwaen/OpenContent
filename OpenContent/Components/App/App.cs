using System;

namespace Satrabel.OpenContent.Components
{
    public class App 
    {
        private static readonly Lazy<App> Lazy = new Lazy<App>(() => new App());
        public static IAppConfig Config => BaseConfig;
        public static IAppServices Services => BaseServices;

        private App()
        {
            Init(new MyConfig(), new MyServices());
        }

        private static IAppConfig _configuration;
        private static IAppServices _serviceConfig;

        private static void Init(IAppConfig config, MyServices services)
        {
            _configuration = config;
            _serviceConfig = services;
        }

        private static IAppConfig BaseConfig
        {
            get
            {
                if (_configuration == null)
                    throw new Exception("AppBase not initialized.  Call AppConfig.Init(config) first.");
                return _configuration;
            }
        }

        private static IAppServices BaseServices
        {
            get
            {
                if (_serviceConfig == null)
                    throw new Exception("AppBase not initialized.  Call AppConfig.Init(config) first.");
                return _serviceConfig;
            }
        }
    }
}