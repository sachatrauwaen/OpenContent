using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Files;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Localization;

namespace Satrabel.OpenContent.Components
{
    public interface IAppServices
    {
        ILogAdapter Logger { get; }
        IIndexAdapter Indexer { get; }
        ILocalizationAdapter Localizer { get; }
        ICacheAdapter CacheAdapter { get; }
        IFileRepositoryAdapter FileRepository { get; }
        IGlobalSettingsRepositoryAdapter GlobalSettings(int tenantId = -1);
        IClientResourceManager ClientResourceManager { get; }
    }
}