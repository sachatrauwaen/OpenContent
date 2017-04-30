using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Files
{
    public interface IFileRepositoryAdapter
    {
        T LoadJsonFileFromCacheOrDisk<T>(FileUri file);
        JToken LoadJsonFileFromDisk(string filename);
        JToken LoadJsonFromFile(FileUri fileUri);
    }
}