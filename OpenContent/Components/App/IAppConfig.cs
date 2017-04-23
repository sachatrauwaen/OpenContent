using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Localization;

namespace Satrabel.OpenContent.Components
{
    public interface IAppConfig
    {
        ILogAdapter LogAdapter { get; }
        IIndexAdapter IndexAdapter { get; }
        ILocalizationAdapter LocalizationAdapter { get; }
        string LuceneIndexFolder { get; }
        string FieldNamePublishStartDate { get; }
        string FieldNamePublishEndDate { get; }
        string FieldNamePublishStatus { get; }
        string Opencontent { get; }
        string DefaultCollection { get; }
        ICacheAdapter CacheAdapter { get; }
        IGlobalSettingsRepositoryAdapter GlobalSettings { get; }
    }
}