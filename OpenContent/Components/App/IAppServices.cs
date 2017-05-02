using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Files;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Localization;

namespace Satrabel.OpenContent.Components
{
    public interface IAppServices
    {
        ILogAdapter Logger { get; }
        IIndexAdapter IndexAdapter { get; }
        ILocalizationAdapter LocalizationAdapter { get; }
        ICacheAdapter CacheAdapter { get; }
        IFileRepositoryAdapter FileRepository { get; }
        IGlobalSettingsRepositoryAdapter GlobalSettings { get; }
        IClientResourceManager ClientResourceManager { get; }
    }
}