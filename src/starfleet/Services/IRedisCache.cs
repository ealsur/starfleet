using System;

namespace starfleet.Services{
    public interface IRedisCache{
        T Get<T>(string cacheKey);
        T Put<T>(string cacheKey, T value, TimeSpan relativeExpiration);
        void Evict<T>(string id);
    }
}