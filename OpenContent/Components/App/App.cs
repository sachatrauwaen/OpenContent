using System;

namespace Satrabel.OpenContent.Components
{
    public class App
    {
        private static readonly App _Instance = new App();
        public static IAppConfig Config => _Instance.BaseConfig;
        public static IAppServices Services => _Instance.BaseServices;

        /// <summary>
        /// This is our Composite Root. Here we inject our dependancies. (see http://blog.ploeh.dk/2011/07/28/CompositionRoot/)
        /// Prevents a default instance of the <see cref="App"/> class from being created.
        /// </summary>
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