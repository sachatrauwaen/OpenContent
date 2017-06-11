using System;

namespace Satrabel.OpenContent.Components
{
    public delegate T GetDelegate<out T>(int contentId);

    public interface ICacheAdapter
    {
        void SetCache(string cacheKey, object objectToCache, string dependentFile);
        void SetCache(string cacheKey, object objectToCache, string[] dependentFiles);
        T GetCachedData<T>(string cacheKey, int cacheTimeInMinutes, Func<object,object> func);
        T GetCache<T>(string cacheKey);
        void ClearCache(string cacheKey);
    }
}