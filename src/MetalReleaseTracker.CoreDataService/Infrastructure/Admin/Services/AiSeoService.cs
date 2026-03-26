using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Services;

public class AiSeoService : IAiSeoService
{
    private const string AnthropicApiUrl = "https://api.anthropic.com/v1/messages";
    private const string AnthropicVersion = "2023-06-01";
    private const string DefaultModel = "claude-sonnet-4-20250514";
    private const int DefaultMaxTokens = 1024;

    private static readonly string DefaultBandPrompt = """
        You are an SEO expert specializing in heavy metal music websites.
        Generate optimized SEO metadata for the following band page.

        Band Name: {{bandName}}
        Genre: {{genre}}
        Country: Ukraine
        Formation Year: {{formationYear}}
        Description: {{description}}
        Album Count: {{albumCount}}

        RULES:
        1. Title must be under 60 characters, include band name and key genre.
        2. Description must be under 155 characters, compelling for search results, include band name, genre, and "Ukrainian".
        3. Keywords: 5-10 comma-separated terms relevant for this band's page (include band name, genre, "Ukrainian metal", album-related terms).
        4. Write in English.
        5. Do NOT use generic filler. Be specific to this band.

        Respond ONLY with JSON:
        {"seoTitle": "string", "seoDescription": "string", "seoKeywords": "keyword1, keyword2, keyword3"}
        """;

    private static readonly string DefaultAlbumPrompt = """
        You are an SEO expert specializing in heavy metal music e-commerce.
        Generate optimized SEO metadata for the following album product page.

        Band Name: {{bandName}}
        Album Title: {{albumTitle}}
        Genre: {{genre}}
        Year: {{year}}
        Media Format: {{media}}
        Price: EUR {{price}}
        Label: {{label}}
        Description: {{description}}

        RULES:
        1. Title must be under 60 characters. Format: "Album by Band - Format | Buy Online" or similar.
        2. Description must be under 155 characters, compelling for search results. Include band, album, format, and call to action.
        3. Keywords: 5-10 comma-separated terms (include band name, album name, genre, format, "buy", "vinyl"/"CD" as relevant).
        4. Write in English.
        5. Focus on purchase intent - this is a product page.

        Respond ONLY with JSON:
        {"seoTitle": "string", "seoDescription": "string", "seoKeywords": "keyword1, keyword2, keyword3"}
        """;

    private readonly CoreDataServiceDbContext _context;
    private readonly IAdminSettingsService _settingsService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AiSeoService> _logger;

    public AiSeoService(
        CoreDataServiceDbContext context,
        IAdminSettingsService settingsService,
        IHttpClientFactory httpClientFactory,
        ILogger<AiSeoService> logger)
    {
        _context = context;
        _settingsService = settingsService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<AiSeoResult> GenerateBandSeoAsync(Guid bandId, CancellationToken cancellationToken = default)
    {
        var band = await _context.Bands
            .AsNoTracking()
            .Where(band => band.Id == bandId)
            .Select(band => new
            {
                band.Id,
                band.Name,
                band.Genre,
                band.FormationYear,
                Description = band.Translations
                    .Where(t => t.LanguageCode == "en")
                    .Select(t => t.Description)
                    .FirstOrDefault(),
                AlbumCount = _context.Albums.Count(album => album.BandId == band.Id),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (band is null)
        {
            return new AiSeoResult { Success = false, Error = "Band not found" };
        }

        var prompt = await GetPromptAsync(SettingKeys.AiSeo.BandPrompt, DefaultBandPrompt, cancellationToken);
        prompt = prompt
            .Replace("{{bandName}}", band.Name)
            .Replace("{{genre}}", band.Genre ?? "Metal")
            .Replace("{{formationYear}}", band.FormationYear?.ToString() ?? "Unknown")
            .Replace("{{description}}", band.Description ?? string.Empty)
            .Replace("{{albumCount}}", band.AlbumCount.ToString());

        var result = await CallClaudeAsync(prompt, cancellationToken);
        if (!result.Success)
        {
            return result;
        }

        var translation = await _context.BandTranslations
            .FirstOrDefaultAsync(
                t => t.BandId == bandId && t.LanguageCode == "en",
                cancellationToken);

        if (translation is not null)
        {
            translation.SeoTitle = Truncate(result.SeoTitle, 160);
            translation.SeoDescription = Truncate(result.SeoDescription, 320);
            translation.SeoKeywords = Truncate(result.SeoKeywords, 500);
        }
        else
        {
            _context.BandTranslations.Add(new Data.Entities.BandTranslationEntity
            {
                Id = Guid.NewGuid(),
                BandId = bandId,
                LanguageCode = "en",
                SeoTitle = Truncate(result.SeoTitle, 160),
                SeoDescription = Truncate(result.SeoDescription, 320),
                SeoKeywords = Truncate(result.SeoKeywords, 500),
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated SEO for band: {BandName}", band.Name);
        return result;
    }

    public async Task<AiSeoResult> GenerateAlbumSeoAsync(Guid albumId, CancellationToken cancellationToken = default)
    {
        var album = await _context.Albums
            .AsNoTracking()
            .Include(album => album.Band)
            .Where(album => album.Id == albumId)
            .Select(album => new
            {
                album.Id,
                album.Name,
                album.CanonicalTitle,
                BandName = album.Band.Name,
                album.Genre,
                album.OriginalYear,
                album.Media,
                album.Price,
                album.Label,
                album.Description,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (album is null)
        {
            return new AiSeoResult { Success = false, Error = "Album not found" };
        }

        var prompt = await GetPromptAsync(SettingKeys.AiSeo.AlbumPrompt, DefaultAlbumPrompt, cancellationToken);
        prompt = prompt
            .Replace("{{bandName}}", album.BandName)
            .Replace("{{albumTitle}}", album.CanonicalTitle ?? album.Name)
            .Replace("{{genre}}", album.Genre ?? "Metal")
            .Replace("{{year}}", album.OriginalYear?.ToString() ?? "Unknown")
            .Replace("{{media}}", album.Media?.ToString() ?? "Unknown")
            .Replace("{{price}}", album.Price.ToString("F2"))
            .Replace("{{label}}", album.Label ?? string.Empty)
            .Replace("{{description}}", album.Description ?? string.Empty);

        var result = await CallClaudeAsync(prompt, cancellationToken);
        if (!result.Success)
        {
            return result;
        }

        var translation = await _context.AlbumTranslations
            .FirstOrDefaultAsync(
                t => t.AlbumId == albumId && t.LanguageCode == "en",
                cancellationToken);

        if (translation is not null)
        {
            translation.SeoTitle = Truncate(result.SeoTitle, 160);
            translation.SeoDescription = Truncate(result.SeoDescription, 320);
            translation.SeoKeywords = Truncate(result.SeoKeywords, 500);
        }
        else
        {
            _context.AlbumTranslations.Add(new Data.Entities.AlbumTranslationEntity
            {
                Id = Guid.NewGuid(),
                AlbumId = albumId,
                LanguageCode = "en",
                SeoTitle = Truncate(result.SeoTitle, 160),
                SeoDescription = Truncate(result.SeoDescription, 320),
                SeoKeywords = Truncate(result.SeoKeywords, 500),
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated SEO for album: {AlbumName} by {BandName}", album.Name, album.BandName);
        return result;
    }

    public async Task<int> GenerateBulkBandSeoAsync(int limit, CancellationToken cancellationToken = default)
    {
        var bandIds = await _context.Bands
            .AsNoTracking()
            .Where(band => !band.Translations.Any(t => t.LanguageCode == "en" && t.SeoTitle != null))
            .OrderBy(band => band.Name)
            .Take(limit)
            .Select(band => band.Id)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var bandId in bandIds)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var result = await GenerateBandSeoAsync(bandId, cancellationToken);
            if (result.Success)
            {
                processed++;
            }

            await Task.Delay(500, cancellationToken);
        }

        _logger.LogInformation("Bulk SEO generation: processed {Count} bands", processed);
        return processed;
    }

    public async Task<int> GenerateBulkAlbumSeoAsync(int limit, CancellationToken cancellationToken = default)
    {
        var albumIds = await _context.Albums
            .AsNoTracking()
            .Where(album => !album.Translations.Any(t => t.LanguageCode == "en" && t.SeoTitle != null))
            .OrderByDescending(album => album.CreatedDate)
            .Take(limit)
            .Select(album => album.Id)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var albumId in albumIds)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var result = await GenerateAlbumSeoAsync(albumId, cancellationToken);
            if (result.Success)
            {
                processed++;
            }

            await Task.Delay(500, cancellationToken);
        }

        _logger.LogInformation("Bulk SEO generation: processed {Count} albums", processed);
        return processed;
    }

    private async Task<string> GetPromptAsync(string key, string defaultValue, CancellationToken cancellationToken)
    {
        return await _settingsService.GetStringSettingAsync(
            SettingCategories.AiSeo,
            key,
            defaultValue,
            cancellationToken);
    }

    private async Task<AiSeoResult> CallClaudeAsync(string prompt, CancellationToken cancellationToken)
    {
        var apiKey = await _settingsService.GetStringSettingAsync(
            SettingCategories.AiSeo,
            SettingKeys.AiSeo.ApiKey,
            string.Empty,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new AiSeoResult { Success = false, Error = "AI API key not configured. Set it in Settings > AI SEO." };
        }

        var model = await _settingsService.GetStringSettingAsync(
            SettingCategories.AiSeo,
            SettingKeys.AiSeo.Model,
            DefaultModel,
            cancellationToken);

        var maxTokens = await _settingsService.GetIntSettingAsync(
            SettingCategories.AiSeo,
            SettingKeys.AiSeo.MaxTokens,
            DefaultMaxTokens,
            cancellationToken);

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestBody = new
            {
                model,
                max_tokens = maxTokens,
                messages = new[]
                {
                    new { role = "user", content = prompt },
                },
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(AnthropicApiUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Claude API error {StatusCode}: {Error}", response.StatusCode, errorBody);
                return new AiSeoResult { Success = false, Error = $"API error: {response.StatusCode}" };
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseClaudeResponse(responseJson);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to call Claude API for SEO generation");
            return new AiSeoResult { Success = false, Error = exception.Message };
        }
    }

    private static AiSeoResult ParseClaudeResponse(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        if (!root.TryGetProperty("content", out var contentArray) || contentArray.GetArrayLength() == 0)
        {
            return new AiSeoResult { Success = false, Error = "Empty response from AI" };
        }

        var text = contentArray[0].GetProperty("text").GetString() ?? string.Empty;

        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}');
        if (jsonStart < 0 || jsonEnd < 0)
        {
            return new AiSeoResult { Success = false, Error = "No JSON found in AI response" };
        }

        var jsonText = text[jsonStart..(jsonEnd + 1)];
        using var resultDoc = JsonDocument.Parse(jsonText);
        var result = resultDoc.RootElement;

        return new AiSeoResult
        {
            Success = true,
            SeoTitle = result.TryGetProperty("seoTitle", out var title) ? title.GetString() : null,
            SeoDescription = result.TryGetProperty("seoDescription", out var description) ? description.GetString() : null,
            SeoKeywords = result.TryGetProperty("seoKeywords", out var keywords) ? keywords.GetString() : null,
        };
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
