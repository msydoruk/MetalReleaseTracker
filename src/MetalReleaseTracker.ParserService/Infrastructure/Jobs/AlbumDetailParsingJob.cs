using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Models;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class AlbumDetailParsingJob
{
    private readonly Func<DistributorCode, IAlbumDetailParser> _detailParserResolver;
    private readonly ICatalogueIndexRepository _catalogueIndexRepository;
    private readonly ICatalogueIndexDetailRepository _catalogueIndexDetailRepository;
    private readonly IImageUploadService _imageUploadService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<AlbumDetailParsingJob> _logger;
    private readonly Random _random = new();

    public AlbumDetailParsingJob(
        Func<DistributorCode, IAlbumDetailParser> detailParserResolver,
        ICatalogueIndexRepository catalogueIndexRepository,
        ICatalogueIndexDetailRepository catalogueIndexDetailRepository,
        IImageUploadService imageUploadService,
        ISettingsService settingsService,
        ILogger<AlbumDetailParsingJob> logger)
    {
        _detailParserResolver = detailParserResolver;
        _catalogueIndexRepository = catalogueIndexRepository;
        _catalogueIndexDetailRepository = catalogueIndexDetailRepository;
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
                await UpsertDetailAsync(entry, albumParsedEvent, cancellationToken);

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

                await HandleParseFailureAsync(entry, cancellationToken);
            }
        }
    }

    private async Task UpsertDetailAsync(
        CatalogueIndexEntity entry,
        AlbumParsedEvent albumParsedEvent,
        CancellationToken cancellationToken)
    {
        var existingDetail = await _catalogueIndexDetailRepository.GetByCatalogueIndexIdAsync(entry.Id, cancellationToken);

        if (existingDetail == null)
        {
            var detail = new CatalogueIndexDetailEntity
            {
                CatalogueIndexId = entry.Id,
                ChangeType = ChangeType.New,
                PublicationStatus = PublicationStatus.Unpublished,
            };

            MapAlbumFieldsToDetail(detail, albumParsedEvent);
            await _catalogueIndexDetailRepository.AddAsync(detail, cancellationToken);
        }
        else if (HasAlbumChanged(existingDetail, albumParsedEvent))
        {
            MapAlbumFieldsToDetail(existingDetail, albumParsedEvent);
            existingDetail.ChangeType = ChangeType.Updated;
            existingDetail.PublicationStatus = PublicationStatus.Unpublished;
            await _catalogueIndexDetailRepository.UpdateAsync(existingDetail, cancellationToken);
        }
        else
        {
            existingDetail.ChangeType = ChangeType.Active;
            await _catalogueIndexDetailRepository.UpdateAsync(existingDetail, cancellationToken);
        }
    }

    private async Task HandleParseFailureAsync(CatalogueIndexEntity entry, CancellationToken cancellationToken)
    {
        try
        {
            await _catalogueIndexRepository.UpdateStatusAsync(entry.Id, CatalogueIndexStatus.Deleted, cancellationToken);

            var existingDetail = await _catalogueIndexDetailRepository.GetByCatalogueIndexIdAsync(entry.Id, cancellationToken);
            if (existingDetail != null)
            {
                existingDetail.ChangeType = ChangeType.Deleted;
                existingDetail.PublicationStatus = PublicationStatus.Unpublished;
                await _catalogueIndexDetailRepository.UpdateAsync(existingDetail, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to handle parse failure for entry {EntryId}.", entry.Id);
        }
    }

    private static void MapAlbumFieldsToDetail(CatalogueIndexDetailEntity detail, AlbumParsedEvent source)
    {
        detail.DistributorCode = source.DistributorCode;
        detail.BandName = source.BandName;
        detail.SKU = source.SKU;
        detail.Name = source.Name;
        detail.Genre = source.Genre;
        detail.Price = source.Price;
        detail.PurchaseUrl = source.PurchaseUrl;
        detail.PhotoUrl = source.PhotoUrl;
        detail.Media = source.Media;
        detail.Label = source.Label;
        detail.Press = source.Press;
        detail.Description = source.Description;
        detail.Status = source.Status;
        detail.CanonicalTitle = source.CanonicalTitle;
        detail.OriginalYear = source.OriginalYear;
    }

    private static bool HasAlbumChanged(CatalogueIndexDetailEntity existing, AlbumParsedEvent parsed)
    {
        return existing.Price != parsed.Price
            || existing.Name != parsed.Name
            || existing.PhotoUrl != parsed.PhotoUrl
            || existing.PurchaseUrl != parsed.PurchaseUrl
            || existing.Genre != parsed.Genre
            || existing.Label != parsed.Label
            || existing.Press != parsed.Press
            || existing.Description != parsed.Description
            || existing.Status != parsed.Status
            || existing.Media != parsed.Media
            || existing.CanonicalTitle != parsed.CanonicalTitle
            || existing.OriginalYear != parsed.OriginalYear;
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
