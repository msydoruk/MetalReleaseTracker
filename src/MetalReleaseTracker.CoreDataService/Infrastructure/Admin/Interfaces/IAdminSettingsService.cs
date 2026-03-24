using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Dtos;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;

public interface IAdminSettingsService
{
    Task<CategorySettingsDto> GetSettingsByCategoryAsync(string category, CancellationToken cancellationToken = default);

    Task<CategorySettingsDto> UpdateSettingsByCategoryAsync(string category, CategorySettingsDto dto, CancellationToken cancellationToken = default);

    Task<int> GetIntSettingAsync(string category, string key, int defaultValue, CancellationToken cancellationToken = default);

    Task<bool> GetBoolSettingAsync(string category, string key, bool defaultValue, CancellationToken cancellationToken = default);

    Task<string> GetStringSettingAsync(string category, string key, string defaultValue, CancellationToken cancellationToken = default);
}
