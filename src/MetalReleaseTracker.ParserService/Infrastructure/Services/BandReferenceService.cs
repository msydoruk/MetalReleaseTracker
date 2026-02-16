using System.Text.Json;
using System.Text.RegularExpressions;
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

    private readonly IBandReferenceRepository _bandReferenceRepository;
    private readonly ISeleniumWebDriverFactory _webDriverFactory;
    private readonly BandReferenceSettings _settings;
    private readonly ILogger<BandReferenceService> _logger;
    private readonly Random _random = new();

    public BandReferenceService(
        IBandReferenceRepository bandReferenceRepository,
        ISeleniumWebDriverFactory webDriverFactory,
        IOptions<BandReferenceSettings> settings,
        ILogger<BandReferenceService> logger)
    {
        _bandReferenceRepository = bandReferenceRepository;
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

            _logger.LogInformation("Navigating to Metal Archives to pass Cloudflare challenge.");
            await Task.Run(() => driver.Navigate().GoToUrl(_settings.MetalArchivesBaseUrl), cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var totalProcessed = 0;
            var offset = 0;
            int totalRecords;

            do
            {
                var url = BuildSearchUrl(offset);
                _logger.LogInformation("Fetching Metal Archives page at offset {Offset}.", offset);

                var json = await FetchJsonViaSelenium(driver, url, cancellationToken);
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
        }
        finally
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

    private static (string BandName, long MaId) ParseBandHtml(string html)
    {
        var hrefMatch = Regex.Match(html, @"href=""[^""]*?/bands/[^/]+/(\d+)""");
        var nameMatch = Regex.Match(html, @">([^<]+)</a>");

        var maId = hrefMatch.Success && long.TryParse(hrefMatch.Groups[1].Value, out var id) ? id : 0;
        var bandName = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : string.Empty;

        return (bandName, maId);
    }

    private static async Task<string> FetchJsonViaSelenium(IWebDriver driver, string url, CancellationToken cancellationToken)
    {
        var jsExecutor = (IJavaScriptExecutor)driver;

        var script = @"
            var callback = arguments[arguments.length - 1];
            fetch(arguments[0], {
                headers: {
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
            .then(function(response) { return response.text(); })
            .then(function(text) { callback(text); })
            .catch(function(error) { callback('ERROR: ' + error.message); });
        ";

        var result = await Task.Run(() => jsExecutor.ExecuteAsyncScript(script, url), cancellationToken);

        var json = result?.ToString() ?? string.Empty;

        if (json.StartsWith("ERROR:", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Selenium fetch failed: {json}");
        }

        return json;
    }
}
