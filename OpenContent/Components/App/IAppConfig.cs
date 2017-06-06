
namespace Satrabel.OpenContent.Components
{
    public interface IAppConfig
    {
        string FieldNamePublishStartDate { get; }
        string FieldNamePublishEndDate { get; }
        string FieldNamePublishStatus { get; }
        string Opencontent { get; }
        string DefaultCollection { get; }
        string ApplicationMapPath { get; }
        string LuceneIndexFolder { get; }
    }
}