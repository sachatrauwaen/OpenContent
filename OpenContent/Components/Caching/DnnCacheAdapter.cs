using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Cache;

namespace Satrabel.OpenContent.Components
{
    public class DnnCacheAdapter : ICacheAdapter
    {
        public T GetCache<T>(string cacheKey)
        {
            return (T)DataCache.GetCache(cacheKey);
        }

        public void SetCache(string cacheKey, object objectToCache, string dependentFile)
        {
            DataCache.SetCache(cacheKey, objectToCache, new DNNCacheDependency(dependentFile));
        }

        public void SetCache(string cacheKey, object objectToCache, string[] dependentFiles)
        {
            DataCache.SetCache(cacheKey, objectToCache, new DNNCacheDependency(dependentFiles));
        }

        public T GetCachedData<T>(string key, int cacheTimeInMinutes, Func<object, object> func)
        {
            var cacheArgs = new CacheItemArgs(key, cacheTimeInMinutes);
            var retval = DataCache.GetCachedData<T>(cacheArgs, func.Invoke);
            return retval;
        }

        public void ClearCache(string cacheKey)
        {
            DataCache.ClearCache(cacheKey);
        }
    }
}