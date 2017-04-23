using System;

namespace Satrabel.OpenContent.Components
{
    public delegate T GetDelegate<out T>(int contentId);

    public interface ICacheAdapter
    {
        T GetCache<T>(string cacheKey);
        void SetCache(string cacheKey, object objectToCache, string dependentFile);
        void SetCache(string cacheKey, object objectToCache, string[] dependentFiles);
        T GetCachedData<T>(string key, int cacheTimeInMinutes, Func<object,object> func);
        void ClearCache(string cacheKey);
    }
}