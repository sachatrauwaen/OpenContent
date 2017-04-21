using System;

namespace Satrabel.OpenContent.Components
{
    public abstract class AppBase
    {
        private static IAppConfig _configuration;

        static AppBase()
        {
            if (_configuration == null)
                throw new Exception("AppBase not initialized.  Call AppConfig.Init(config) first.");
        }

        internal static void Init(IAppConfig config)
        {
            _configuration = config;
        }

        public IAppConfig Config => _configuration;
    }
}