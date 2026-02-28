using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Services.Configuration;

namespace MetalReleaseTracker.ParserService.Infrastructure.Services;

public class BandReferenceService : IBandReferenceService
{
    private const int PageSize = 200;
    private const int MaxFetchRetries = 3;

    private readonly IBandReferenceRepository _bandReferenceRepository;
    private readonly IBandDiscographyRepository _bandDiscographyRepository;
    private readonly IFlareSolverrClient _flareSolverrClient;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<BandReferenceService> _logger;
    private readonly Random _random = new();

    public BandReferenceService(
        IBandReferenceRepository bandReferenceRepository,
        IBandDiscographyRepository bandDiscographyRepository,
        IFlareSolverrClient flareSolverrClient,
        ISettingsService settingsService,
        ILogger<BandReferenceService> logger)
    {
        _bandReferenceRepository = bandReferenceRepository;
        _bandDiscographyRepository = bandDiscographyRepository;
        _flareSolverrClient = flareSolverrClient;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task SyncUkrainianBandsAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetBandReferenceSettingsAsync(cancellationToken);

        _logger.LogInformation("Starting Ukrainian bands sync from Metal Archives.");

        var sessionId = await _flareSolverrClient.CreateSessionAsync(cancellationToken);

        try
        {
            var totalProcessed = 0;
            var offset = 0;
            int totalRecords;

            do
            {
                var url = BuildSearchUrl(settings, offset);
                _logger.LogInformation("Fetching Metal Archives page at offset {Offset}.", offset);

                var json = await FetchJsonWithRetry(url, sessionId, cancellationToken);
                var (bands, total) = ParseMetalArchivesResponse(json);
                totalRecords = total;

                foreach (var band in bands)
                {
                    await _bandReferenceRepository.UpsertAsync(band, cancellationToken);
                    totalProcessed++;
                }

                _logger.LogInformation(
                    "Processed {Count} bands (offset {Offset}/{Total}).",
                    bands.Count,
                    offset,
                    totalRecords);

                offset += PageSize;

                if (offset < totalRecords)
                {
                    await DelayBetweenRequests(settings, cancellationToken);
                }
            }
            while (offset < totalRecords);

            _logger.LogInformation("Ukrainian bands sync completed. Total bands processed: {Total}.", totalProcessed);

            await SyncDiscographiesAsync(settings, sessionId, cancellationToken);
        }
        finally
        {
            await _flareSolverrClient.DestroySessionAsync(sessionId, cancellationToken);
        }
    }

    private async Task SyncDiscographiesAsync(BandReferenceSettings settings, string sessionId, CancellationToken cancellationToken)
    {
        var bands = await _bandReferenceRepository.GetAllAsync(cancellationToken);
        _logger.LogInformation("Starting discography sync for {Count} bands.", bands.Count);

        var syncedCount = 0;

        foreach (var band in bands)
        {
            try
            {
                var url = $"{settings.MetalArchivesBaseUrl}/band/discography/id/{band.MetalArchivesId}/tab/all";
                var html = await _flareSolverrClient.GetPageContentAsync(url, sessionId, cancellationToken);
                var entries = ParseDiscographyHtml(html, band.Id);

                await _bandDiscographyRepository.ReplaceForBandAsync(band.Id, entries, cancellationToken);
                syncedCount++;

                _logger.LogInformation(
                    "Synced discography for band '{BandName}' (MA ID: {MaId}): {AlbumCount} albums.",
                    band.BandName,
                    band.MetalArchivesId,
                    entries.Count);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to sync discography for band '{BandName}' (MA ID: {MaId}). Skipping.",
                    band.BandName,
                    band.MetalArchivesId);
            }

            await DelayBetweenRequests(settings, cancellationToken);
        }

        _logger.LogInformation("Discography sync completed. Synced {Count}/{Total} bands.", syncedCount, bands.Count);
    }

    private async Task<string> FetchJsonWithRetry(string url, string sessionId, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxFetchRetries; attempt++)
        {
            try
            {
                var html = await _flareSolverrClient.GetPageContentAsync(url, sessionId, cancellationToken);
                var json = ExtractJsonFromHtml(html);

                if (json.TrimStart().StartsWith('{'))
                {
                    return json;
                }

                var preview = json.Length > 200 ? json[..200] : json;
                _logger.LogWarning(
                    "Metal Archives returned non-JSON response (attempt {Attempt}/{MaxRetries}). Preview: {Preview}",
                    attempt,
                    MaxFetchRetries,
                    preview);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to fetch Metal Archives page (attempt {Attempt}/{MaxRetries}): {Url}",
                    attempt,
                    MaxFetchRetries,
                    url);
            }

            if (attempt < MaxFetchRetries)
            {
                await Task.Delay(5000 * attempt, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Metal Archives did not return valid JSON after {MaxFetchRetries} attempts.");
    }

    private string BuildSearchUrl(BandReferenceSettings settings, int offset)
    {
        return $"{settings.MetalArchivesBaseUrl}/search/ajax-advanced/searching/bands/"
            + $"?bandName=&country={settings.SyncCountryCode}&status="
            + $"&iDisplayStart={offset}&iDisplayLength={PageSize}";
    }

    private (List<BandReferenceEntity> Bands, int TotalRecords) ParseMetalArchivesResponse(string json)
    {
        var bands = new List<BandReferenceEntity>();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var totalRecords = root.GetProperty("iTotalRecords").GetInt32();
        var data = root.GetProperty("aaData");

        foreach (var row in data.EnumerateArray())
        {
            if (row.GetArrayLength() < 3)
            {
                continue;
            }

            var bandHtml = row[0].GetString() ?? string.Empty;
            var genreText = row[1].GetString() ?? string.Empty;

            var (bandName, maId) = ParseBandHtml(bandHtml);
            if (maId == 0 || string.IsNullOrWhiteSpace(bandName))
            {
                continue;
            }

            bands.Add(new BandReferenceEntity
            {
                BandName = bandName,
                MetalArchivesId = maId,
                Genre = genreText,
                LastSyncedAt = DateTime.UtcNow
            });
        }

        return (bands, totalRecords);
    }

    private async Task DelayBetweenRequests(BandReferenceSettings settings, CancellationToken cancellationToken)
    {
        var delayMs = _random.Next(settings.MinRequestDelayMs, settings.MaxRequestDelayMs);
        await Task.Delay(delayMs, cancellationToken);
    }

    private static string ExtractJsonFromHtml(string html)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var preNode = htmlDoc.DocumentNode.SelectSingleNode("//pre");
        if (preNode != null)
        {
            return System.Net.WebUtility.HtmlDecode(preNode.InnerText);
        }

        var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
        if (bodyNode != null)
        {
            var bodyText = System.Net.WebUtility.HtmlDecode(bodyNode.InnerText).Trim();
            if (bodyText.StartsWith('{'))
            {
                return bodyText;
            }
        }

        return html;
    }

    private static (string BandName, long MaId) ParseBandHtml(string html)
    {
        var hrefMatch = Regex.Match(html, @"href=""[^""]*?/bands/[^/]+/(\d+)""");
        var nameMatch = Regex.Match(html, @">([^<]+)</a>");

        var maId = hrefMatch.Success && long.TryParse(hrefMatch.Groups[1].Value, out var id) ? id : 0;
        var bandName = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : string.Empty;

        return (bandName, maId);
    }

    private static List<BandDiscographyEntity> ParseDiscographyHtml(string html, Guid bandReferenceId)
    {
        var entries = new List<BandDiscographyEntity>();
        var seenNormalizedTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rows = doc.DocumentNode.SelectNodes("//table[contains(@class,'discog')]//tr");
        if (rows == null)
        {
            return entries;
        }

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("td");
            if (cells == null || cells.Count < 3)
            {
                continue;
            }

            var titleNode = cells[0].SelectSingleNode(".//a");
            var albumTitle = titleNode != null
                ? System.Net.WebUtility.HtmlDecode(titleNode.InnerText.Trim())
                : System.Net.WebUtility.HtmlDecode(cells[0].InnerText.Trim());

            var albumType = System.Net.WebUtility.HtmlDecode(cells[1].InnerText.Trim());
            var yearText = cells[2].InnerText.Trim();

            if (string.IsNullOrWhiteSpace(albumTitle))
            {
                continue;
            }

            var normalizedTitle = AlbumTitleNormalizer.Normalize(albumTitle);
            if (!seenNormalizedTitles.Add(normalizedTitle))
            {
                continue;
            }

            int? year = int.TryParse(yearText, out var parsedYear) ? parsedYear : null;

            entries.Add(new BandDiscographyEntity
            {
                Id = Guid.NewGuid(),
                BandReferenceId = bandReferenceId,
                AlbumTitle = albumTitle,
                NormalizedAlbumTitle = normalizedTitle,
                AlbumType = albumType,
                Year = year
            });
        }

        return entries;
    }
}
