using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Services;

public class SettingsSeedService : ISettingsSeedService
{
    private const string DefaultSystemPrompt = """
        You are a metal music expert. Determine if the following band originates from Ukraine.

        Band: {{bandName}}
        Album: {{albumTitle}}

        {{discography}}

        RULES:
        1. If a discography IS provided above, each entry is prefixed with a UUID in square brackets.
           It lists all known albums of a Ukrainian band with this name from Metal Archives.
           Multiple bands often share the same name.
           - If "{{albumTitle}}" matches or closely matches an entry in the discography → isUkrainian: true,
             and return the UUID of the matched entry as matchedAlbumId.
           - IMPORTANT: Metal Archives often lists album titles in the band's native language
             (Russian, Ukrainian, or other Cyrillic scripts), while distributors may use the English
             translation, transliteration, or international title. Consider cross-language equivalents
             when matching. For example, a Cyrillic title and its English translation are the SAME album.
           - If "{{albumTitle}}" does NOT appear in the discography even after considering translations
             and cross-language equivalents → this is most likely a DIFFERENT band
             with the same name → isUkrainian: false, matchedAlbumId: null.
        2. If NO discography is provided, use your general knowledge of the metal scene to determine
           the band's country of origin. Consider: band name language, Metal Archives, Discogs,
           Encyclopaedia Metallum, band websites, and other reliable sources.
           In this case matchedAlbumId must be null.

        Consider the band's country of origin (not individual members' nationality).

        Respond ONLY with JSON:
        {"isUkrainian": true/false, "confidence": 0.0-1.0, "analysis": "brief reasoning", "matchedAlbumId": "exact-uuid-from-list-or-null"}
        """;

    private const string DefaultModel = "claude-sonnet-4-20250514";
    private const int DefaultMaxTokens = 1024;
    private const int DefaultMaxConcurrentRequests = 5;
    private const int DefaultDelayBetweenBatchesMs = 1000;

    private static readonly (DistributorCode Code, string Name, string Url)[] DefaultParsingSources =
    [
        (DistributorCode.OsmoseProductions, "osmoseproductions.com", "https://www.osmoseproductions.com/liste/?what=label&tete=osmose&srt=2&fmt=11"),
        (DistributorCode.Drakkar, "drakkar666.com", "https://www.drakkar666.com/product-category/audio/cds/"),
        (DistributorCode.BlackMetalVendor, "black-metal-vendor.com", "https://black-metal-vendor.com/en/Audio-Records-A-Z/Compact-Disc:::2_122.html"),
        (DistributorCode.BlackMetalStore, "blackmetalstore.com", "https://blackmetalstore.com/categoria-produto/cds/"),
        (DistributorCode.NapalmRecords, "napalmrecords.com", "https://napalmrecords.com/english/music/cds?product_list_dir=desc&product_list_order=release_date"),
        (DistributorCode.SeasonOfMist, "shop.season-of-mist.com", "https://shop.season-of-mist.com/music?cat=3"),
        (DistributorCode.ParagonRecords, "paragonrecords.org", "https://www.paragonrecords.org/collections/cd"),
    ];

    private readonly ParserServiceDbContext _context;
    private readonly ILogger<SettingsSeedService> _logger;

    public SettingsSeedService(
        ParserServiceDbContext context,
        ILogger<SettingsSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedAiAgentAsync(cancellationToken);
        await SeedParsingSourcesAsync(cancellationToken);
        await SeedCategorySettingsAsync(cancellationToken);
    }

    private async Task SeedAiAgentAsync(CancellationToken cancellationToken)
    {
        var hasAgents = await _context.AiAgents.AnyAsync(cancellationToken);
        if (hasAgents)
        {
            return;
        }

        var agent = new AiAgentEntity
        {
            Id = Guid.NewGuid(),
            Name = "Ukrainian Band Verification",
            Description = "Verifies whether a band is Ukrainian and corrects album titles using Metal Archives discography data.",
            SystemPrompt = DefaultSystemPrompt,
            Model = DefaultModel,
            MaxTokens = DefaultMaxTokens,
            MaxConcurrentRequests = DefaultMaxConcurrentRequests,
            DelayBetweenBatchesMs = DefaultDelayBetweenBatchesMs,
            ApiKey = string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.AiAgents.Add(agent);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded default AI agent: {AgentName}.", agent.Name);
    }

    private async Task SeedParsingSourcesAsync(CancellationToken cancellationToken)
    {
        var hasSources = await _context.ParsingSources.AnyAsync(cancellationToken);
        if (hasSources)
        {
            return;
        }

        foreach (var (code, name, url) in DefaultParsingSources)
        {
            var entity = new ParsingSourceEntity
            {
                Id = Guid.NewGuid(),
                DistributorCode = code,
                Name = name,
                ParsingUrl = url,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.ParsingSources.Add(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} parsing sources.", DefaultParsingSources.Length);
    }

    private async Task SeedCategorySettingsAsync(CancellationToken cancellationToken)
    {
        var hasSettings = await _context.Settings.AnyAsync(cancellationToken);
        if (hasSettings)
        {
            return;
        }

        var settings = new List<SettingEntity>
        {
            CreateSetting("BandReference", "MetalArchivesBaseUrl", "https://www.metal-archives.com"),
            CreateSetting("BandReference", "SyncCountryCode", "UA"),
            CreateSetting("BandReference", "MinRequestDelayMs", "3000"),
            CreateSetting("BandReference", "MaxRequestDelayMs", "5000"),
            CreateSetting("FlareSolverr", "BaseUrl", "http://flaresolverr:8191"),
            CreateSetting("FlareSolverr", "MaxTimeoutMs", "60000"),
            CreateSetting("GeneralParser", "MinDelayBetweenRequestsSeconds", "1"),
            CreateSetting("GeneralParser", "MaxDelayBetweenRequestsSeconds", "5"),
        };

        _context.Settings.AddRange(settings);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} category settings.", settings.Count);
    }

    private static SettingEntity CreateSetting(string category, string key, string value)
    {
        return new SettingEntity
        {
            Key = key,
            Value = value,
            Category = category,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
