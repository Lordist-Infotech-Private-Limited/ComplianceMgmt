using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace ComplianceMgmt.Api.Services
{
    public class HybridCache : IHybridCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;

        public HybridCache(IMemoryCache memoryCache, IDistributedCache distributedCache)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            // Try getting from memory cache
            if (_memoryCache.TryGetValue(key, out T value))
            {
                return value;
            }

            // Fallback to distributed cache
            var distributedValue = await _distributedCache.GetStringAsync(key);
            if (distributedValue != null)
            {
                value = JsonSerializer.Deserialize<T>(distributedValue);
                _memoryCache.Set(key, value); // Cache back in memory
            }

            return value;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan cacheDuration)
        {
            // Store in memory cache
            _memoryCache.Set(key, value, cacheDuration);

            // Store in distributed cache
            var serializedValue = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration
            });
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            _distributedCache.RemoveAsync(key);
        }
    }

    public interface IHybridCache
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan cacheDuration);
        void Remove(string key);
    }

}
