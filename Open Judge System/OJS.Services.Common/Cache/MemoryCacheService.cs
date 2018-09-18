namespace OJS.Services.Common.Cache
{
    using System;
    using System.Web;
    using System.Web.Caching;

    public class MemoryCacheService : ICacheService
    {
        public T Get<T>(string cacheId, Func<T> getItemCallback, int? cacheSeconds)
            where T : class
        {
            if (HttpRuntime.Cache.Get(cacheId) is T item)
            {
                return item;
            }

            item = getItemCallback();

            SetCache(cacheId, item, cacheSeconds);

            return item;
        }

        public void Remove(string cacheId) => HttpRuntime.Cache.Remove(cacheId);

        private static void SetCache<T>(string cacheId, T item, int? cacheSeconds)
        {
            var absoluteExpiration = cacheSeconds.HasValue
                ? DateTime.Now.AddSeconds(cacheSeconds.Value)
                : Cache.NoAbsoluteExpiration;

            HttpContext.Current.Cache.Add(
                cacheId,
                item,
                null,
                absoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.Default,
                null);
        }
    }
}