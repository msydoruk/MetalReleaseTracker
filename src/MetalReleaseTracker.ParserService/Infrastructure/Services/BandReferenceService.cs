using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Services.Configuration;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;

namespace MetalReleaseTracker.ParserService.Infrastructure.Services;

public class BandReferenceService : IBandReferenceService
{
    private const int PageSize = 200;
    private const int CloudflareWaitSeconds = 15;
    private const int MaxFetchRetries = 5;
    private const int RetryDelayMs = 5000;
    private const int PageLoadWaitSeconds = 3;

    private readonly IBandReferenceRepository _bandReferenceRepository;
    private readonly IBandDiscographyRepository _bandDiscographyRepository;
    private readonly ISeleniumWebDriverFactory _webDriverFactory;
    private readonly BandReferenceSettings _settings;
    private readonly ILogger<BandReferenceService> _logger;
    private readonly Random _random = new();

    public BandReferenceService(
        IBandReferenceRepository bandReferenceRepository,
        IBandDiscographyRepository bandDiscographyRepository,
        ISeleniumWebDriverFactory webDriverFactory,
        IOptions<BandReferenceSettings> settings,
        ILogger<BandReferenceService> logger)
    {
        _bandReferenceRepository = bandReferenceRepository;
        _bandDiscographyRepository = bandDiscographyRepository;
        _webDriverFactory = webDriverFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SyncUkrainianBandsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Ukrainian bands sync from Metal Archives.");

        IWebDriver? driver = null;
        try
        {
            driver = _webDriverFactory.CreateDriver();
            _logger.LogInformation("Created Selenium WebDriver for Metal Archives sync.");

            await PassCloudflareChallenge(driver, cancellationToken);

            var totalProcessed = 0;
            var offset = 0;
            int totalRecords;

            do
            {
                var url = BuildSearchUrl(offset);
                _logger.LogInformation("Fetching Metal Archives page at offset {Offset}.", offset);

                var json = await FetchPageWithRetry(driver, url, cancellationToken);
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
                    await DelayBetweenRequests(cancellationToken);
                }
            }
            while (offset < totalRecords);

            _logger.LogInformation("Ukrainian bands sync completed. Total bands processed: {Total}.", totalProcessed);

            await SyncDiscographiesAsync(driver, cancellationToken);
        }
        finally
        {
            DisposeDriver(driver);
        }
    }

    private async Task PassCloudflareChallenge(IWebDriver driver, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Navigating to Metal Archives to pass Cloudflare challenge.");
        await Task.Run(() => driver.Navigate().GoToUrl(_settings.MetalArchivesBaseUrl), cancellationToken);
        _logger.LogInformation("Waiting {Seconds}s for Cloudflare challenge to resolve.", CloudflareWaitSeconds);
        await Task.Delay(TimeSpan.FromSeconds(CloudflareWaitSeconds), cancellationToken);
    }

    private async Task SyncDiscographiesAsync(IWebDriver driver, CancellationToken cancellationToken)
    {
        var bands = await _bandReferenceRepository.GetAllAsync(cancellationToken);
        _logger.LogInformation("Starting discography sync for {Count} bands.", bands.Count);

        var syncedCount = 0;

        foreach (var band in bands)
        {
            try
            {
                var url = $"{_settings.MetalArchivesBaseUrl}/band/discography/id/{band.MetalArchivesId}/tab/all";
                var html = await LoadPageContent(driver, url, cancellationToken);
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

            await DelayBetweenRequests(cancellationToken);
        }

        _logger.LogInformation("Discography sync completed. Synced {Count}/{Total} bands.", syncedCount, bands.Count);
    }

    private async Task<string> LoadPageContent(IWebDriver driver, string url, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Run(() => driver.Navigate().GoToUrl(url), cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(PageLoadWaitSeconds), cancellationToken);

        var pageSource = driver.PageSource;
        if (string.IsNullOrEmpty(pageSource))
        {
            throw new InvalidOperationException($"Empty page source from Metal Archives: {url}");
        }

        return ExtractBodyText(driver, pageSource);
    }

    private async Task<string> FetchPageWithRetry(IWebDriver driver, string url, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxFetchRetries; attempt++)
        {
            try
            {
                var content = await LoadPageContent(driver, url, cancellationToken);

                if (content.TrimStart().StartsWith('{'))
                {
                    return content;
                }

                var preview = content.Length > 200 ? content[..200] : content;
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
                    "Failed to load Metal Archives page (attempt {Attempt}/{MaxRetries}): {Url}",
                    attempt,
                    MaxFetchRetries,
                    url);
            }

            if (attempt < MaxFetchRetries)
            {
                await Task.Delay(RetryDelayMs * attempt, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Metal Archives did not return valid JSON after {MaxFetchRetries} attempts.");
    }

    private string BuildSearchUrl(int offset)
    {
        return $"{_settings.MetalArchivesBaseUrl}/search/ajax-advanced/searching/bands/"
            + $"?bandName=&country={_settings.SyncCountryCode}&status="
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

    private async Task DelayBetweenRequests(CancellationToken cancellationToken)
    {
        var delayMs = _random.Next(_settings.MinRequestDelayMs, _settings.MaxRequestDelayMs);
        await Task.Delay(delayMs, cancellationToken);
    }

    private void DisposeDriver(IWebDriver? driver)
    {
        if (driver != null)
        {
            try
            {
                driver.Quit();
                driver.Dispose();
                _logger.LogInformation("Disposed Selenium WebDriver for Metal Archives sync.");
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Error disposing Selenium WebDriver.");
            }
        }
    }

    private static string ExtractBodyText(IWebDriver driver, string pageSource)
    {
        try
        {
            var jsExecutor = (IJavaScriptExecutor)driver;
            var bodyText = jsExecutor.ExecuteScript("return document.body.innerText")?.ToString();
            if (!string.IsNullOrEmpty(bodyText))
            {
                return bodyText;
            }
        }
        catch
        {
            // Fall through to HTML parsing
        }

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(pageSource);
        var preNode = htmlDoc.DocumentNode.SelectSingleNode("//pre");

        return preNode != null ? preNode.InnerText : pageSource;
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
