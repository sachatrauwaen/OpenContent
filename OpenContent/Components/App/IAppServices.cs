using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Files;
using Satrabel.OpenContent.Components.Localization;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components
{
    public interface IAppServices
    {
        ILogAdapter Logger { get; }
        ILuceneIndexAdapter LuceneIndex { get; }
        ILocalizationAdapter Localizer { get; }
        ICacheAdapter CacheAdapter { get; }
        IFileRepositoryAdapter FileRepository { get; }
        IGlobalSettingsRepositoryAdapter GlobalSettings(int tenantId = -1);
        IClientResourceManager ClientResourceManager { get; }
    }
}