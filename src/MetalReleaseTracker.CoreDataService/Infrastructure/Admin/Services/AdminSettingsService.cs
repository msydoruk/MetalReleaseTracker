using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Services;

public class AdminSettingsService : IAdminSettingsService
{
    private readonly CoreDataServiceDbContext _context;

    public AdminSettingsService(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<CategorySettingsDto> GetSettingsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var settings = await _context.Settings
            .AsNoTracking()
            .Where(setting => setting.Category == category)
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value, cancellationToken);

        return new CategorySettingsDto(settings);
    }

    public async Task<CategorySettingsDto> UpdateSettingsByCategoryAsync(string category, CategorySettingsDto dto, CancellationToken cancellationToken = default)
    {
        var existingSettings = await _context.Settings
            .Where(setting => setting.Category == category)
            .ToListAsync(cancellationToken);

        foreach (var (key, value) in dto.Settings)
        {
            var existing = existingSettings.FirstOrDefault(setting => setting.Key == key);
            if (existing != null)
            {
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.Settings.Add(new SettingEntity
                {
                    Key = key,
                    Value = value,
                    Category = category,
                    UpdatedAt = DateTime.UtcNow,
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await GetSettingsByCategoryAsync(category, cancellationToken);
    }

    public async Task<int> GetIntSettingAsync(string category, string key, int defaultValue, CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsDictionaryAsync(category, cancellationToken);
        return settings.TryGetValue(key, out var value) && int.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    public async Task<bool> GetBoolSettingAsync(string category, string key, bool defaultValue, CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsDictionaryAsync(category, cancellationToken);
        return settings.TryGetValue(key, out var value) && bool.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    public async Task<string> GetStringSettingAsync(string category, string key, string defaultValue, CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsDictionaryAsync(category, cancellationToken);
        return settings.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value)
            ? value
            : defaultValue;
    }

    private async Task<Dictionary<string, string>> GetSettingsDictionaryAsync(string category, CancellationToken cancellationToken)
    {
        return await _context.Settings
            .AsNoTracking()
            .Where(setting => setting.Category == category)
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value, cancellationToken);
    }
}
