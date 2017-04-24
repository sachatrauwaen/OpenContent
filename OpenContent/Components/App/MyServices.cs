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
        public ILocalizationAdapter LocalizationAdapter => new DnnLocalizationAdapter();
        public ILogAdapter LogAdapter => DnnLogAdapter.GetLogAdapter(App.Config.Opencontent);
        public IIndexAdapter IndexAdapter => new LuceneIndexAdapter(@"App_Data\OpenContent\lucene_index");
        public ICacheAdapter CacheAdapter => new DnnCacheAdapter();
        public IGlobalSettingsRepositoryAdapter GlobalSettings => new DnnGlobalSettingsRepositoryAdapter(PortalSettings.Current.PortalId);
    }
}