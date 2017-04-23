using System;
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
    /// <seealso cref="Satrabel.OpenContent.Components.IAppServices" />
    public class MyServices : IAppServices
    {

        public string LuceneIndexFolder => @"App_Data\OpenContent\lucene_index";

        public ILocalizationAdapter LocalizationAdapter => new DnnLocalizationAdapter();
        public ILogAdapter LogAdapter => DnnLogAdapter.GetLogAdapter(App.Config.Opencontent);
        public IIndexAdapter IndexAdapter => new LuceneIndexAdapter();
        public ICacheAdapter CacheAdapter => new DnnCacheAdapter();


        private IGlobalSettingsRepositoryAdapter _globalSettingsRepository;
        public IGlobalSettingsRepositoryAdapter GlobalSettings
        {
            get
            {
                return _globalSettingsRepository ??
                       (_globalSettingsRepository = new DnnGlobalSettingsRepositoryAdapter(PortalSettings.Current.PortalId));
            }
        }
    }
}