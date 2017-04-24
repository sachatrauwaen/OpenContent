using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Localization;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Lucene;

namespace Satrabel.OpenContent.Components
{
    /// <summary>
    /// Configure all Services here
    /// </summary>
    public class MyServices : IAppServices
    {

        private IGlobalSettingsRepositoryAdapter _globalSettingsAdapter;
        private ICacheAdapter _cacheAdapter;
        private ILocalizationAdapter _localizationAdapter;

        public string LuceneIndexFolder => @"App_Data\OpenContent\lucene_index";

        public ILocalizationAdapter LocalizationAdapter => _localizationAdapter ?? (_localizationAdapter = new DnnLocalizationAdapter());
        public ILogAdapter LogAdapter => DnnLogAdapter.GetLogAdapter(App.Config.Opencontent);
        public IIndexAdapter IndexAdapter => new LuceneIndexAdapter();
        public ICacheAdapter CacheAdapter => _cacheAdapter ?? (_cacheAdapter = new DnnCacheAdapter());
        public IGlobalSettingsRepositoryAdapter GlobalSettings => _globalSettingsAdapter ?? (_globalSettingsAdapter = new DnnGlobalSettingsRepositoryAdapter(PortalSettings.Current.PortalId));
    }
}