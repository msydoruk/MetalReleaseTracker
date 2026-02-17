using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class CatalogueIndexJob
{
    private readonly Func<DistributorCode, IListingParser> _listingParserResolver;
    private readonly ICatalogueIndexRepository _catalogueIndexRepository;
    private readonly IBandDiscographyRepository _bandDiscographyRepository;
    private readonly GeneralParserSettings _generalParserSettings;
    private readonly ILogger<CatalogueIndexJob> _logger;
    private readonly Random _random = new();

    public CatalogueIndexJob(
        Func<DistributorCode, IListingParser> listingParserResolver,
        ICatalogueIndexRepository catalogueIndexRepository,
        IBandDiscographyRepository bandDiscographyRepository,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<CatalogueIndexJob> logger)
    {
        _listingParserResolver = listingParserResolver;
        _catalogueIndexRepository = catalogueIndexRepository;
        _bandDiscographyRepository = bandDiscographyRepository;
        _generalParserSettings = generalParserSettings.Value;
        _logger = logger;
    }

    public async Task RunCatalogueIndexJob(ParserDataSource parserDataSource, CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteIndexingAsync(parserDataSource, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Catalogue indexing was cancelled for distributor: {DistributorCode}.",
                parserDataSource.DistributorCode);
            throw;
        }
        catch (ObjectDisposedException)
        {
            _logger.LogWarning(
                "Catalogue indexing was interrupted (CancellationTokenSource disposed) for distributor: {DistributorCode}.",
                parserDataSource.DistributorCode);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error occurred while indexing catalogue for distributor: {DistributorCode}.",
                parserDataSource.DistributorCode);
            throw;
        }
    }

    private async Task ExecuteIndexingAsync(ParserDataSource parserDataSource, CancellationToken cancellationToken)
    {
        var parser = _listingParserResolver(parserDataSource.DistributorCode);
        var currentUrl = parserDataSource.ParsingUrl;
        var totalIndexed = 0;

        var bandAlbumMap = await _bandDiscographyRepository.GetAllGroupedByBandNameAsync(cancellationToken);

        _logger.LogInformation(
            "Starting catalogue indexing for distributor: {DistributorCode}. Loaded {BandCount} Ukrainian bands with discography data for matching.",
            parserDataSource.DistributorCode,
            bandAlbumMap.Count);

        do
        {
            _logger.LogInformation(
                "Parsing listing page for {DistributorCode}: {Url}.",
                parserDataSource.DistributorCode,
                currentUrl);

            var result = await parser.ParseListingsAsync(currentUrl, cancellationToken);

            foreach (var listing in result.Listings)
            {
                var status = DetermineStatus(bandAlbumMap, listing.BandName, listing.AlbumTitle);

                var entity = new CatalogueIndexEntity
                {
                    DistributorCode = parserDataSource.DistributorCode,
                    BandName = listing.BandName,
                    AlbumTitle = listing.AlbumTitle,
                    RawTitle = listing.RawTitle,
                    DetailUrl = listing.DetailUrl,
                    MediaType = listing.MediaType,
                    Status = status
                };

                await _catalogueIndexRepository.UpsertAsync(entity, cancellationToken);
                totalIndexed++;
            }

            _logger.LogInformation(
                "Indexed {Count} listings from page. Total so far: {Total}.",
                result.Listings.Count,
                totalIndexed);

            if (result.HasMorePages)
            {
                currentUrl = result.NextPageUrl;
                await DelayBetweenPages(cancellationToken);
            }
            else
            {
                currentUrl = null;
            }
        }
        while (!string.IsNullOrEmpty(currentUrl));

        _logger.LogInformation(
            "Catalogue indexing completed for {DistributorCode}. Total entries: {Total}.",
            parserDataSource.DistributorCode,
            totalIndexed);
    }

    private async Task DelayBetweenPages(CancellationToken cancellationToken)
    {
        var delaySeconds = _random.Next(
            _generalParserSettings.MinDelayBetweenRequestsSeconds,
            _generalParserSettings.MaxDelayBetweenRequestsSeconds);

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
    }

    public static CatalogueIndexStatus DetermineStatus(
        Dictionary<string, HashSet<string>> bandAlbumMap,
        string bandName,
        string albumTitle)
    {
        var albumTitles = FindAlbumTitles(bandAlbumMap, bandName);
        if (albumTitles == null)
        {
            return CatalogueIndexStatus.NotRelevant;
        }

        if (albumTitles.Count == 0)
        {
            return CatalogueIndexStatus.PendingReview;
        }

        var normalizedAlbumTitle = AlbumTitleNormalizer.Normalize(albumTitle);

        return albumTitles.Contains(normalizedAlbumTitle)
            ? CatalogueIndexStatus.Relevant
            : CatalogueIndexStatus.PendingReview;
    }

    private static HashSet<string>? FindAlbumTitles(
        Dictionary<string, HashSet<string>> bandAlbumMap,
        string bandName)
    {
        if (bandAlbumMap.TryGetValue(bandName, out var exactMatch))
        {
            return exactMatch;
        }

        foreach (var kvp in bandAlbumMap)
        {
            if (kvp.Key.Contains(bandName, StringComparison.OrdinalIgnoreCase)
                || bandName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        return null;
    }
}
