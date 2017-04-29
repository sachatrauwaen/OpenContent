namespace Satrabel.OpenContent.Components.Files
{
    public interface IFileRepositoryAdapter
    {
        T LoadJsonFileFromCacheOrDisk<T>(FileUri file);
    }
}