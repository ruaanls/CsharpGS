using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace WeatherAlertAPI.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _options;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
            _options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);
            if (data == null) return default;
            return JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value)
        {
            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, _options);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            // Nota: Em produção, você pode querer usar Redis SCAN para isso
            // Este é um exemplo simplificado
            await _cache.RemoveAsync(prefix + "*");
        }
    }
}
