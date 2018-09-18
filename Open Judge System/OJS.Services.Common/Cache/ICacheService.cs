namespace OJS.Services.Common.Cache
{
    using System;

    public interface ICacheService : IService
    {
        T Get<T>(string cacheId, Func<T> getItemCallback, int? cacheSeconds)
            where T : class;

        void Remove(string cacheId);
    }
}