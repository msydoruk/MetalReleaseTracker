using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public sealed class RateLimitSettingsCache
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;

    public RateLimitSettingsCache(IServiceScopeFactory scopeFactory, IMemoryCache cache)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
    }

    public int CatalogPermitLimit => GetCachedInt(SettingKeys.RateLimiting.CatalogPermitLimit, 100);

    public int CatalogWindowMinutes => GetCachedInt(SettingKeys.RateLimiting.CatalogWindowMinutes, 1);

    public int AuthPermitLimit => GetCachedInt(SettingKeys.RateLimiting.AuthPermitLimit, 10);

    public int AuthWindowMinutes => GetCachedInt(SettingKeys.RateLimiting.AuthWindowMinutes, 1);

    private int GetCachedInt(string key, int defaultValue)
    {
        return _cache.GetOrCreate($"RateLimit:{key}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            using var scope = _scopeFactory.CreateScope();
            var settingsService = scope.ServiceProvider.GetRequiredService<IAdminSettingsService>();
            return settingsService.GetIntSettingAsync(
                SettingCategories.RateLimiting, key, defaultValue, CancellationToken.None)
                .GetAwaiter().GetResult();
        });
    }
}
