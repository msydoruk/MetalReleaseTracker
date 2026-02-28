using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Models;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using Newtonsoft.Json;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class AlbumDetailParsingJob
{
    private readonly Func<DistributorCode, IAlbumDetailParser> _detailParserResolver;
    private readonly ICatalogueIndexRepository _catalogueIndexRepository;
    private readonly IParsingSessionRepository _parsingSessionRepository;
    private readonly IAlbumParsedEventRepository _albumParsedEventRepository;
    private readonly IImageUploadService _imageUploadService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<AlbumDetailParsingJob> _logger;
    private readonly Random _random = new();

    public AlbumDetailParsingJob(
        Func<DistributorCode, IAlbumDetailParser> detailParserResolver,
        ICatalogueIndexRepository catalogueIndexRepository,
        IParsingSessionRepository parsingSessionRepository,
        IAlbumParsedEventRepository albumParsedEventRepository,
        IImageUploadService imageUploadService,
        ISettingsService settingsService,
        ILogger<AlbumDetailParsingJob> logger)
    {
        _detailParserResolver = detailParserResolver;
        _catalogueIndexRepository = catalogueIndexRepository;
        _parsingSessionRepository = parsingSessionRepository;
        _albumParsedEventRepository = albumParsedEventRepository;
        _imageUploadService = imageUploadService;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task RunDetailParsingJob(ParserDataSource parserDataSource, CancellationToken cancellationToken)
    {
        try
        {
            var source = await _settingsService.GetParsingSourceByCodeAsync(parserDataSource.DistributorCode, cancellationToken);
            if (source == null || !source.IsEnabled)
            {
                _logger.LogInformation(
                    "Skipping album detail parsing for disabled distributor: {DistributorCode}.",
                    parserDataSource.DistributorCode);
                return;
            }

            await ParseRelevantEntries(parserDataSource, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Album detail parsing job was cancelled for distributor: {DistributorCode}.",
                parserDataSource.DistributorCode);
            throw;
        }
        catch (ObjectDisposedException)
        {
            _logger.LogWarning(
                "Album detail parsing job was interrupted (CancellationTokenSource disposed) for distributor: {DistributorCode}.",
                parserDataSource.DistributorCode);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error occurred during album detail parsing job for distributor: {DistributorCode}.",
                parserDataSource.DistributorCode);
            throw;
        }
    }

    private async Task ParseRelevantEntries(ParserDataSource parserDataSource, CancellationToken cancellationToken)
    {
        var generalParserSettings = await _settingsService.GetGeneralParserSettingsAsync(cancellationToken);
        var distributorCode = parserDataSource.DistributorCode;
        var relevantEntries = await _catalogueIndexRepository.GetByStatusesWithDiscographyAsync(
            distributorCode,
            [CatalogueIndexStatus.AiVerified],
            cancellationToken);

        if (relevantEntries.Count == 0)
        {
            _logger.LogInformation("No relevant entries for {DistributorCode}.", distributorCode);
            return;
        }

        var parsingSession = await GetOrCreateParsingSessionAsync(distributorCode, cancellationToken);
        var parser = _detailParserResolver(distributorCode);

        _logger.LogInformation(
            "Parsing {Count} relevant entries for {DistributorCode}.",
            relevantEntries.Count,
            distributorCode);

        foreach (var entry in relevantEntries)
        {
            try
            {
                _logger.LogInformation(
                    "Scraping detail page for {BandName} - {AlbumTitle} ({DistributorCode}).",
                    entry.BandName,
                    entry.AlbumTitle,
                    distributorCode);

                var albumParsedEvent = await parser.ParseAlbumDetailAsync(entry.DetailUrl, cancellationToken);

                if (!string.IsNullOrEmpty(albumParsedEvent.SKU))
                {
                    albumParsedEvent.SKU = $"{distributorCode}-{albumParsedEvent.SKU}";
                }

                if (entry.MediaType.HasValue)
                {
                    albumParsedEvent.Media = entry.MediaType;
                }

                if (entry.BandDiscography != null)
                {
                    albumParsedEvent.CanonicalTitle = entry.BandDiscography.AlbumTitle;
                    albumParsedEvent.OriginalYear = entry.BandDiscography.Year;
                }

                await ProcessAlbumImageAsync(albumParsedEvent, cancellationToken);

                await _albumParsedEventRepository.AddAsync(
                    parsingSession.Id,
                    JsonConvert.SerializeObject(albumParsedEvent),
                    cancellationToken);

                await _catalogueIndexRepository.UpdateStatusAsync(entry.Id, CatalogueIndexStatus.Processed, cancellationToken);

                _logger.LogInformation(
                    "Parsed album: {SKU} - {BandName}. Distributor: {DistributorCode}.",
                    albumParsedEvent.SKU,
                    albumParsedEvent.BandName,
                    distributorCode);

                await DelayBetweenRequests(generalParserSettings, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to parse detail page for entry {EntryId} ({DetailUrl}).",
                    entry.Id,
                    entry.DetailUrl);
            }
        }

        await _parsingSessionRepository.UpdateParsingStatus(
            parsingSession.Id,
            AlbumParsingStatus.Parsed,
            cancellationToken);
    }

    private async Task<ParsingSessionEntity> GetOrCreateParsingSessionAsync(
        DistributorCode distributorCode,
        CancellationToken cancellationToken)
    {
        return await _parsingSessionRepository.GetIncompleteAsync(distributorCode, cancellationToken)
            ?? await _parsingSessionRepository.AddAsync(distributorCode, cancellationToken);
    }

    private async Task ProcessAlbumImageAsync(AlbumParsedEvent albumParsedEvent, CancellationToken cancellationToken)
    {
        var imageUploadRequest = new ImageUploadRequest
        {
            ImageUrl = albumParsedEvent.PhotoUrl,
            AlbumSku = albumParsedEvent.SKU ?? Guid.NewGuid().ToString(),
            DistributorCode = albumParsedEvent.DistributorCode,
            AlbumName = albumParsedEvent.Name,
            BandName = albumParsedEvent.BandName
        };

        var uploadResult = await _imageUploadService.UploadAlbumImageAsync(imageUploadRequest, cancellationToken);

        if (uploadResult.IsSuccess)
        {
            albumParsedEvent.PhotoUrl = uploadResult.BlobPath;
        }
    }

    private async Task DelayBetweenRequests(GeneralParserSettings generalParserSettings, CancellationToken cancellationToken)
    {
        var delaySeconds = _random.Next(
            generalParserSettings.MinDelayBetweenRequestsSeconds,
            generalParserSettings.MaxDelayBetweenRequestsSeconds);

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
    }
}
