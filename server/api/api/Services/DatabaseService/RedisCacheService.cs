using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace api.Services.DatabaseService;

public class RedisCacheService(ILogger<RedisCacheService> logger, IDistributedCache cache)
{
    private readonly ILogger<RedisCacheService> _logger = logger;
    private readonly IDistributedCache _cache = cache;
    
    /// <summary>
    /// Attempts to get a value from cache. 
    /// Returns (true, value) if found, (false, default) if not.
    /// </summary>
    public async Task<(bool Success, T? Value)> TryGet<T>(string key)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key);

            if (string.IsNullOrWhiteSpace(cachedData))
            {
                return (false, default);
            }

            var result = JsonSerializer.Deserialize<T>(cachedData);
            return (true, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache key: {Key}", key);
            return (false, default);
        }
    }

    /// <summary>
    /// Sets a value in cache with optional expiration settings.
    /// </summary>
    public async Task<bool> Set<T>(string key, T value, DistributedCacheEntryOptions? options = null)
    {
        try
        {
            // If no options provided, set a default (e.g., 1 hour)
            options ??= new DistributedCacheEntryOptions 
            { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(80) 
            };

            var jsonData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, jsonData, options);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Removes a key from cache.
    /// </summary>
    public async Task Remove(string key)
    {
        await _cache.RemoveAsync(key);
    }
}
