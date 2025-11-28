using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using NotesApi.Helpers;

public class CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger, ClaimsPrincipal user) : ICacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<CacheService> _logger = logger;
    private readonly string? _userId = user.FindFirst(Constants.CLAIM_USERID)?.Value;

    public bool TryGetValue<T>(string key, out T? value) => _memoryCache.TryGetValue($"{_userId}{key}", out value);

    public void Set<T>(string key, T? value) => _memoryCache.Set($"{_userId}{key}", value);

    public void Remove(string key) => _memoryCache.Remove($"{_userId}{key}");

    public void Clear()
    {
        if (_memoryCache is MemoryCache concreteMemoryCache)
        {
            concreteMemoryCache.Clear();
            _logger.Log(LogLevel.Information, "Cache cleared");
        }
        else
        {
            _logger.Log(LogLevel.Warning, "IMemoryCache instance is not a MemoryCache; cannot use Clear() directly.");
        }
    }
}