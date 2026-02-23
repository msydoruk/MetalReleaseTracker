using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class CatalogueIndexJob
{
    private readonly Func<DistributorCode, IListingParser> _listingParserResolver;
    private readonly ICatalogueIndexRepository _catalogueIndexRepository;
    private readonly IBandDiscographyRepository _bandDiscographyRepository;
    private readonly IBandReferenceRepository _bandReferenceRepository;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<CatalogueIndexJob> _logger;
    private readonly Random _random = new();

    public CatalogueIndexJob(
        Func<DistributorCode, IListingParser> listingParserResolver,
        ICatalogueIndexRepository catalogueIndexRepository,
        IBandDiscographyRepository bandDiscographyRepository,
        IBandReferenceRepository bandReferenceRepository,
        ISettingsService settingsService,
        ILogger<CatalogueIndexJob> logger)
    {
        _listingParserResolver = listingParserResolver;
        _catalogueIndexRepository = catalogueIndexRepository;
        _bandDiscographyRepository = bandDiscographyRepository;
        _bandReferenceRepository = bandReferenceRepository;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task RunCatalogueIndexJob(ParserDataSource parserDataSource, CancellationToken cancellationToken)
    {
        try
        {
            var source = await _settingsService.GetParsingSourceByCodeAsync(parserDataSource.DistributorCode, cancellationToken);
            if (source == null || !source.IsEnabled)
            {
                _logger.LogInformation(
                    "Skipping catalogue indexing for disabled distributor: {DistributorCode}.",
                    parserDataSource.DistributorCode);
                return;
            }

            parserDataSource.ParsingUrl = source.ParsingUrl;
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
        var generalParserSettings = await _settingsService.GetGeneralParserSettingsAsync(cancellationToken);
        var parser = _listingParserResolver(parserDataSource.DistributorCode);
        var currentUrl = parserDataSource.ParsingUrl;
        var totalIndexed = 0;

        var bandNames = await _bandDiscographyRepository.GetAllBandNamesAsync(cancellationToken);
        var bandReferenceLookup = await BuildBandReferenceLookupAsync(cancellationToken);

        _logger.LogInformation(
            "Starting catalogue indexing for distributor: {DistributorCode}. Loaded {BandCount} Ukrainian band names for matching.",
            parserDataSource.DistributorCode,
            bandNames.Count);

        do
        {
            _logger.LogInformation(
                "Parsing listing page for {DistributorCode}: {Url}.",
                parserDataSource.DistributorCode,
                currentUrl);

            var result = await parser.ParseListingsAsync(currentUrl, cancellationToken);

            foreach (var listing in result.Listings)
            {
                var status = DetermineStatus(bandNames, listing.BandName);
                bandReferenceLookup.TryGetValue(listing.BandName, out var bandReferenceId);

                var entity = new CatalogueIndexEntity
                {
                    DistributorCode = parserDataSource.DistributorCode,
                    BandName = listing.BandName,
                    AlbumTitle = listing.AlbumTitle,
                    RawTitle = listing.RawTitle,
                    DetailUrl = listing.DetailUrl,
                    MediaType = listing.MediaType,
                    Status = status,
                    BandReferenceId = status == CatalogueIndexStatus.Relevant ? bandReferenceId : null,
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
                await DelayBetweenPages(generalParserSettings, cancellationToken);
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

    private async Task<Dictionary<string, Guid>> BuildBandReferenceLookupAsync(CancellationToken cancellationToken)
    {
        var allBandReferences = await _bandReferenceRepository.GetAllAsync(cancellationToken);
        var lookup = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        foreach (var bandReference in allBandReferences)
        {
            lookup.TryAdd(bandReference.BandName, bandReference.Id);
        }

        return lookup;
    }

    private async Task DelayBetweenPages(GeneralParserSettings generalParserSettings, CancellationToken cancellationToken)
    {
        var delaySeconds = _random.Next(
            generalParserSettings.MinDelayBetweenRequestsSeconds,
            generalParserSettings.MaxDelayBetweenRequestsSeconds);

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
    }

    public static CatalogueIndexStatus DetermineStatus(
        HashSet<string> bandNames,
        string bandName)
    {
        return IsBandInCanonicalList(bandNames, bandName)
            ? CatalogueIndexStatus.Relevant
            : CatalogueIndexStatus.NotRelevant;
    }

    private static bool IsBandInCanonicalList(HashSet<string> bandNames, string bandName)
    {
        if (bandNames.Contains(bandName))
        {
            return true;
        }

        return false;
    }
}
