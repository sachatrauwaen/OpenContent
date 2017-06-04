using System;

namespace Satrabel.OpenContent.Components
{
    public class App
    {
        private static readonly Lazy<App> Lazy = new Lazy<App>(() => new App());
        public static IAppConfig Config => Lazy.Value.BaseConfig;
        public static IAppServices Services => Lazy.Value.BaseServices;

        private App()
        {
            Init(new MyConfig(), new MyServices());
        }

        private static IAppConfig _configuration;
        private static IAppServices _serviceConfig;

        internal static void Init(IAppConfig config, IAppServices services)
        {
            _configuration = config;
            _serviceConfig = services;
        }

        private IAppConfig BaseConfig
        {
            get
            {
                if (_configuration == null)
                    throw new Exception("AppBase not initialized.  Call AppConfig.Init(config) first.");
                return _configuration;
            }
        }

        private IAppServices BaseServices
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