using System;

namespace Satrabel.OpenContent.Components
{
    public abstract class AppBase
    {
        private static IAppConfig _configuration;
        private static IAppServices _serviceConfig;

        static AppBase()
        {
            if (_configuration == null)
                throw new Exception("AppBase not initialized.  Call AppConfig.Init(config) first.");
        }

        internal static void Init(IAppConfig config, MyServices services)
        {
            _configuration = config;
            _serviceConfig = services;
        }

        public IAppConfig Config => _configuration;
        public IAppServices Services => _serviceConfig;
    }
}