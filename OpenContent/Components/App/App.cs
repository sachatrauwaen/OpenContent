using System;

namespace Satrabel.OpenContent.Components
{
    public class App
    {
        private static readonly App _Instance = new App();
        public static IAppConfig Config => _Instance.BaseConfig;
        public static IAppServices Services => _Instance.BaseServices;

        private App()
        {
            Init(new MyConfig(), new MyServices());
        }

        private static IAppConfig _configuration;
        private static IAppServices _serviceConfig;


        /// <summary>
        /// Initializes the specified configuration.
        /// </summary>
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
                    throw new Exception("App not initialized.  Call App.Init(config) first.");
                return _configuration;
            }
        }

        private IAppServices BaseServices
        {
            get
            {
                if (_serviceConfig == null)
                    throw new Exception("App not initialized.  Call App.Init(config) first.");
                return _serviceConfig;
            }
        }
    }
}