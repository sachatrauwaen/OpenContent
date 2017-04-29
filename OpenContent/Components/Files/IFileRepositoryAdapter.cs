namespace Satrabel.OpenContent.Components.Files
{
    public interface IFileRepositoryAdapter
    {
        T LoadDeserializedJsonFileFromCacheOrDisk<T>(FileUri file);
    }
}