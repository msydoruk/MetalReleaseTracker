using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Services.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Services;

public class SettingsService : ISettingsService
{
    private readonly ParserServiceDbContext _context;

    public SettingsService(ParserServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<AiAgentDto>> GetAiAgentsAsync(CancellationToken cancellationToken)
    {
        return await _context.AiAgents
            .AsNoTracking()
            .OrderByDescending(agent => agent.IsActive)
            .ThenBy(agent => agent.Name)
            .Select(agent => MapToAiAgentDto(agent))
            .ToListAsync(cancellationToken);
    }

    public async Task<AiAgentDto?> GetAiAgentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var agent = await _context.AiAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(agent => agent.Id == id, cancellationToken);

        return agent == null ? null : MapToAiAgentDto(agent);
    }

    public async Task<AiAgentEntity?> GetActiveAiAgentAsync(CancellationToken cancellationToken)
    {
        return await _context.AiAgents
            .FirstOrDefaultAsync(agent => agent.IsActive, cancellationToken);
    }

    public async Task<AiAgentDto> CreateAiAgentAsync(CreateAiAgentDto dto, CancellationToken cancellationToken)
    {
        if (dto.IsActive)
        {
            await DeactivateAllAgentsAsync(cancellationToken);
        }

        var entity = new AiAgentEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            SystemPrompt = dto.SystemPrompt,
            Model = dto.Model,
            MaxTokens = dto.MaxTokens,
            MaxConcurrentRequests = dto.MaxConcurrentRequests,
            DelayBetweenBatchesMs = dto.DelayBetweenBatchesMs,
            ApiKey = dto.ApiKey,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.AiAgents.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToAiAgentDto(entity);
    }

    public async Task<AiAgentDto?> UpdateAiAgentAsync(Guid id, UpdateAiAgentDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.AiAgents.FirstOrDefaultAsync(agent => agent.Id == id, cancellationToken);
        if (entity == null)
        {
            return null;
        }

        if (dto.IsActive && !entity.IsActive)
        {
            await DeactivateAllAgentsAsync(cancellationToken);
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.SystemPrompt = dto.SystemPrompt;
        entity.Model = dto.Model;
        entity.MaxTokens = dto.MaxTokens;
        entity.MaxConcurrentRequests = dto.MaxConcurrentRequests;
        entity.DelayBetweenBatchesMs = dto.DelayBetweenBatchesMs;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(dto.ApiKey))
        {
            entity.ApiKey = dto.ApiKey;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToAiAgentDto(entity);
    }

    public async Task<bool> DeleteAiAgentAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.AiAgents.FirstOrDefaultAsync(agent => agent.Id == id, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.AiAgents.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<ParsingSourceDto>> GetParsingSourcesAsync(CancellationToken cancellationToken)
    {
        return await _context.ParsingSources
            .AsNoTracking()
            .OrderBy(source => source.DistributorCode)
            .Select(source => new ParsingSourceDto
            {
                Id = source.Id,
                DistributorCode = source.DistributorCode,
                Name = source.Name,
                ParsingUrl = source.ParsingUrl,
                IsEnabled = source.IsEnabled,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ParsingSourceDto?> UpdateParsingSourceAsync(Guid id, UpdateParsingSourceDto dto, CancellationToken cancellationToken)
    {
        var entity = await _context.ParsingSources.FirstOrDefaultAsync(source => source.Id == id, cancellationToken);
        if (entity == null)
        {
            return null;
        }

        entity.ParsingUrl = dto.ParsingUrl;
        entity.IsEnabled = dto.IsEnabled;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ParsingSourceDto
        {
            Id = entity.Id,
            DistributorCode = entity.DistributorCode,
            Name = entity.Name,
            ParsingUrl = entity.ParsingUrl,
            IsEnabled = entity.IsEnabled,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    public async Task<CategorySettingsDto> GetSettingsByCategoryAsync(string category, CancellationToken cancellationToken)
    {
        var settings = await _context.Settings
            .AsNoTracking()
            .Where(setting => setting.Category == category)
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value, cancellationToken);

        return new CategorySettingsDto(settings);
    }

    public async Task<CategorySettingsDto> UpdateSettingsByCategoryAsync(string category, CategorySettingsDto dto, CancellationToken cancellationToken)
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

    public async Task<List<ParsingSourceEntity>> GetEnabledParsingSourcesAsync(CancellationToken cancellationToken)
    {
        return await _context.ParsingSources
            .AsNoTracking()
            .Where(source => source.IsEnabled)
            .OrderBy(source => source.DistributorCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<ParsingSourceEntity?> GetParsingSourceByCodeAsync(DistributorCode distributorCode, CancellationToken cancellationToken)
    {
        return await _context.ParsingSources
            .AsNoTracking()
            .FirstOrDefaultAsync(source => source.DistributorCode == distributorCode, cancellationToken);
    }

    public async Task<GeneralParserSettings> GetGeneralParserSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await GetSettingsDictionaryAsync("GeneralParser", cancellationToken);

        return new GeneralParserSettings
        {
            MinDelayBetweenRequestsSeconds = GetIntSetting(settings, "MinDelayBetweenRequestsSeconds", 1),
            MaxDelayBetweenRequestsSeconds = GetIntSetting(settings, "MaxDelayBetweenRequestsSeconds", 5),
        };
    }

    public async Task<BandReferenceSettings> GetBandReferenceSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await GetSettingsDictionaryAsync("BandReference", cancellationToken);

        return new BandReferenceSettings
        {
            MetalArchivesBaseUrl = GetStringSetting(settings, "MetalArchivesBaseUrl", "https://www.metal-archives.com"),
            SyncCountryCode = GetStringSetting(settings, "SyncCountryCode", "UA"),
            MinRequestDelayMs = GetIntSetting(settings, "MinRequestDelayMs", 3000),
            MaxRequestDelayMs = GetIntSetting(settings, "MaxRequestDelayMs", 5000),
        };
    }

    public async Task<FlareSolverrSettings> GetFlareSolverrSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await GetSettingsDictionaryAsync("FlareSolverr", cancellationToken);

        return new FlareSolverrSettings
        {
            BaseUrl = GetStringSetting(settings, "BaseUrl", "http://flaresolverr:8191"),
            MaxTimeoutMs = GetIntSetting(settings, "MaxTimeoutMs", 60000),
        };
    }

    private static AiAgentDto MapToAiAgentDto(AiAgentEntity entity)
    {
        return new AiAgentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SystemPrompt = entity.SystemPrompt,
            Model = entity.Model,
            MaxTokens = entity.MaxTokens,
            MaxConcurrentRequests = entity.MaxConcurrentRequests,
            DelayBetweenBatchesMs = entity.DelayBetweenBatchesMs,
            HasApiKey = !string.IsNullOrEmpty(entity.ApiKey),
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    private async Task DeactivateAllAgentsAsync(CancellationToken cancellationToken)
    {
        var activeAgents = await _context.AiAgents
            .Where(agent => agent.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var agent in activeAgents)
        {
            agent.IsActive = false;
            agent.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task<Dictionary<string, string>> GetSettingsDictionaryAsync(string category, CancellationToken cancellationToken)
    {
        return await _context.Settings
            .AsNoTracking()
            .Where(setting => setting.Category == category)
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value, cancellationToken);
    }

    private static int GetIntSetting(Dictionary<string, string> settings, string key, int defaultValue)
    {
        return settings.TryGetValue(key, out var value) && int.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    private static string GetStringSetting(Dictionary<string, string> settings, string key, string defaultValue)
    {
        return settings.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value)
            ? value
            : defaultValue;
    }
}
