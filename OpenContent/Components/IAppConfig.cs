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
    }
}