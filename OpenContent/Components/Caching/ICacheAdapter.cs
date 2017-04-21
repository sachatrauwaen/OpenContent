using System;
using DotNetNuke.Common.Utilities;
using Satrabel.OpenContent.Components.Indexing;

namespace Satrabel.OpenContent.Components
{
    public interface ICacheAdapter
    {
        object GetCache(string cacheKey);
        void SetCache(string cacheKey, object objectToCache, string dependentFile);
        void SetCache(string cacheKey, object objectToCache, string[] dependentFiles);
        T GetCachedData<T>(string key, int cacheTimeInMinutes, Func<object, T> func);
        void ClearCache(string cacheKey);
    }
}