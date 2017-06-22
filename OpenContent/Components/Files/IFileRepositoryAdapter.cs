using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Files
{
    public interface IFileRepositoryAdapter
    {
        T LoadJsonFileFromCacheOrDisk<T>(FileUri file);
        JToken LoadJsonFromCacheOrDisk(FileUri fileUri);
        JToken LoadJsonFileFromDisk(string filename);
    }
}