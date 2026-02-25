using Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Service
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache) => _cache = cache;

        public async Task Delete(string key) => await _cache.RemoveAsync(key);

        public async Task<string?> Get(string key) => await _cache.GetStringAsync(key);

        public async Task Set(string key, string value, double hours)
        {
            DistributedCacheEntryOptions options = new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(hours),
            };

            await _cache.SetStringAsync(key, value, options);
        }

        public async Task SetMinutes(string key, string value, int minutes)
        {
            DistributedCacheEntryOptions options = new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes),
            };

            await _cache.SetStringAsync(key, value, options);
        }
    }
}
