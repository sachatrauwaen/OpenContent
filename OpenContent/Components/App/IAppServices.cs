using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Localization;

namespace Satrabel.OpenContent.Components
{
    public interface IAppServices
    {
        ILogAdapter LogAdapter { get; }
        IIndexAdapter IndexAdapter { get; }
        ILocalizationAdapter LocalizationAdapter { get; }
        string LuceneIndexFolder { get; }
        ICacheAdapter CacheAdapter { get; }
        IGlobalSettingsRepositoryAdapter GlobalSettings { get; }
    }
}