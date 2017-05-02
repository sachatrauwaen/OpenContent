using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Files;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Localization;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Settings;

namespace Satrabel.OpenContent.Components
{
    /// <summary>
    /// Configure all Services here
    /// </summary>
    public class MyServices : IAppServices
    {
        public ILocalizationAdapter LocalizationAdapter => new DnnLocalizationAdapter();
        public ILogAdapter Logger => DnnLogAdapter.GetLogAdapter(App.Config.Opencontent);
        public IIndexAdapter IndexAdapter => new LuceneIndexAdapter(@"App_Data\OpenContent\lucene_index");
        public ICacheAdapter CacheAdapter => new DnnCacheAdapter();
        public IFileRepositoryAdapter FileRepository => new DnnFileRepositoryAdapter();
        public IGlobalSettingsRepositoryAdapter GlobalSettings(int tenantId = -1)
        {
            if (tenantId < 0)
                return new DnnGlobalSettingsRepositoryAdapter(PortalSettings.Current.PortalId);
            else
                return new DnnGlobalSettingsRepositoryAdapter(tenantId);
        }

        public IClientResourceManager ClientResourceManager => new DnnClientResourceManager();
    }
}