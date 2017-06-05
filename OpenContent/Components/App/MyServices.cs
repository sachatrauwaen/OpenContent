using System;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Files;
using Satrabel.OpenContent.Components.Localization;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Settings;

namespace Satrabel.OpenContent.Components
{
    /// <summary>
    /// Configure all Services here
    /// </summary>
    public class MyServices : IAppServices
    {

        /// <summary>
        /// Initializes the <see cref="MyServices"/> class.
        /// All Thread-safe / cross-portal services go here. 
        /// </summary>
        /// <remarks>
        /// We use Lazy() to make sure service objects don't get created immediatly, as this might cause loops when the underlying services call other services.
        /// Also Lazy() ensures that objects are not created when never called. But that is not relevant with cross-portal services as they always get used at some point.
        /// </remarks>
        static MyServices()
        {
            _Indexer = new Lazy<ILuceneIndexAdapter>(() => new DnnLuceneIndexAdapter(@"App_Data\OpenContent\lucene_index"));
            _Localizer = new Lazy<ILocalizationAdapter>(() => new DnnLocalizationAdapter());
            _Logger = new Lazy<ILogAdapter>(() => DnnLogAdapter.GetLogAdapter(App.Config.Opencontent));
            _Cacher = new Lazy<ICacheAdapter>(() => new DnnCacheAdapter());
            _FileRepos = new Lazy<IFileRepositoryAdapter>(() => new DnnFileRepositoryAdapter());
            _ClientResourcer = new Lazy<IClientResourceManager>(() => new DnnClientResourceManager());
        }

        // static private variables for Thread-safe / cross-portal services
        private static readonly Lazy<ILuceneIndexAdapter> _Indexer;
        private static readonly Lazy<ILocalizationAdapter> _Localizer;
        private static readonly Lazy<ILogAdapter> _Logger;
        private static readonly Lazy<ICacheAdapter> _Cacher;
        private static readonly Lazy<IFileRepositoryAdapter> _FileRepos;
        private static readonly Lazy<IClientResourceManager> _ClientResourcer;


        // static variables for Thread-safe / cross-portal services
        public ILuceneIndexAdapter LuceneIndex => _Indexer.Value;
        public ILocalizationAdapter Localizer => _Localizer.Value;
        public ILogAdapter Logger => _Logger.Value;
        public ICacheAdapter CacheAdapter => _Cacher.Value;
        public IFileRepositoryAdapter FileRepository => _FileRepos.Value;
        public IClientResourceManager ClientResourceManager => _ClientResourcer.Value;


        public IGlobalSettingsRepositoryAdapter GlobalSettings(int tenantId = -1)
        {
            if (tenantId < 0)
                return new DnnGlobalSettingsRepositoryAdapter(PortalSettings.Current.PortalId);
            else
                return new DnnGlobalSettingsRepositoryAdapter(tenantId);
        }
    }
}