using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Models;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Services;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class AlbumDetailParsingJob
{
    private readonly Func<DistributorCode, IAlbumDetailParser> _detailParserResolver;
    private readonly ICatalogueIndexRepository _catalogueIndexRepository;
    private readonly ICatalogueIndexDetailRepository _catalogueIndexDetailRepository;
    private readonly IImageUploadService _imageUploadService;
    private readonly ISettingsService _settingsService;
    private readonly IParsingProgressTracker _progressTracker;
    private readonly ILogger<AlbumDetailParsingJob> _logger;
    private readonly Random _random = new();

    public AlbumDetailParsingJob(
        Func<DistributorCode, IAlbumDetailParser> detailParserResolver,
        ICatalogueIndexRepository catalogueIndexRepository,
        ICatalogueIndexDetailRepository catalogueIndexDetailRepository,
        IImageUploadService imageUploadService,
        ISettingsService settingsService,
        IParsingProgressTracker progressTracker,
        ILogger<AlbumDetailParsingJob> logger)
    {
        _detailParserResolver = detailParserResolver;
        _catalogueIndexRepository = catalogueIndexRepository;
        _catalogueIndexDetailRepository = catalogueIndexDetailRepository;
        _imageUploadService = imageUploadService;
        _settingsService = settingsService;
        _progressTracker = progressTracker;
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
            [CatalogueIndexStatus.AiVerified, CatalogueIndexStatus.ZeroPriced],
            cancellationToken);

        if (relevantEntries.Count == 0)
        {
            _logger.LogInformation("No relevant entries for {DistributorCode}.", distributorCode);
            return;
        }

        var parser = _detailParserResolver(distributorCode);
        var runId = Guid.NewGuid();

        _logger.LogInformation(
            "Parsing {Count} relevant entries for {DistributorCode}.",
            relevantEntries.Count,
            distributorCode);

        _progressTracker.StartRun(runId, ParsingJobType.DetailParsing, distributorCode, relevantEntries.Count);

        try
        {
            foreach (var entry in relevantEntries)
            {
                var itemDescription = $"{entry.BandName} - {entry.AlbumTitle}";

                try
                {
                    _logger.LogInformation(
                        "Scraping detail page for {BandName} - {AlbumTitle} ({DistributorCode}).",
                        entry.BandName,
                        entry.AlbumTitle,
                        distributorCode);

                    var albumParsedEvent = await parser.ParseAlbumDetailAsync(entry.DetailUrl, cancellationToken);

                    albumParsedEvent.SKU = $"{(int)distributorCode}-{AlbumParsingHelper.GenerateSkuFromUrl(entry.DetailUrl)}";

                    if (entry.MediaType.HasValue)
                    {
                        albumParsedEvent.Media = entry.MediaType;
                    }

                    if (entry.BandDiscography != null)
                    {
                        albumParsedEvent.CanonicalTitle = entry.BandDiscography.AlbumTitle;
                        albumParsedEvent.OriginalYear = entry.BandDiscography.Year;
                    }

                    var existingDetail = await _catalogueIndexDetailRepository.GetByCatalogueIndexIdAsync(entry.Id, cancellationToken);
                    if (existingDetail == null)
                    {
                        await ProcessAlbumImageAsync(albumParsedEvent, parser, cancellationToken);
                    }

                    var (changeType, isZeroPriced) = await UpsertDetailAsync(entry, existingDetail, albumParsedEvent, cancellationToken);

                    _logger.LogInformation(
                        "Parsed album: {SKU} - {BandName}. Distributor: {DistributorCode}.",
                        albumParsedEvent.SKU,
                        albumParsedEvent.BandName,
                        distributorCode);

                    var category = isZeroPriced
                        ? "zero-priced"
                        : changeType switch
                        {
                            ChangeType.New => "new",
                            ChangeType.Updated => "updated",
                            _ => "active",
                        };
                    _progressTracker.ItemProcessed(runId, itemDescription, category);

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

                    _progressTracker.ItemFailed(runId, itemDescription, exception.Message, "deleted");

                    await HandleParseFailureAsync(entry, cancellationToken);
                }
            }

            _progressTracker.CompleteRun(runId);
        }
        catch (OperationCanceledException)
        {
            _progressTracker.FailRun(runId, "Cancelled");
            throw;
        }
        catch (Exception exception)
        {
            _progressTracker.FailRun(runId, exception.Message);
            throw;
        }
    }

    private async Task<(ChangeType ChangeType, bool IsZeroPriced)> UpsertDetailAsync(
        CatalogueIndexEntity entry,
        CatalogueIndexDetailEntity? existingDetail,
        AlbumParsedEvent albumParsedEvent,
        CancellationToken cancellationToken)
    {
        if (existingDetail == null)
        {
            var publicationStatus = DeterminePublicationStatus(albumParsedEvent.Price, entry.Status);
            var detail = new CatalogueIndexDetailEntity
            {
                CatalogueIndexId = entry.Id,
                ChangeType = ChangeType.New,
                PublicationStatus = publicationStatus,
            };

            MapAlbumFieldsToDetail(detail, albumParsedEvent, entry);

            if (publicationStatus == PublicationStatus.SkippedZeroPrice)
            {
                await _catalogueIndexRepository.UpdateStatusAsync(entry.Id, CatalogueIndexStatus.ZeroPriced, cancellationToken);
                _logger.LogWarning(
                    "Album {BandName} - {AlbumTitle} has zero price, marking as ZeroPriced.",
                    entry.BandName,
                    entry.AlbumTitle);
            }

            await _catalogueIndexDetailRepository.AddAsync(detail, cancellationToken);
            return (ChangeType.New, publicationStatus == PublicationStatus.SkippedZeroPrice);
        }

        var changeReason = DetectChangeReason(existingDetail, albumParsedEvent);
        if (changeReason != null)
        {
            var existingPhotoUrl = existingDetail.PhotoUrl;
            MapAlbumFieldsToDetail(existingDetail, albumParsedEvent, entry);
            existingDetail.PhotoUrl = existingPhotoUrl;
            var publicationStatus = DeterminePublicationStatus(existingDetail.Price, entry.Status);
            existingDetail.ChangeType = ChangeType.Updated;
            existingDetail.ChangeReason = changeReason;
            existingDetail.PublicationStatus = publicationStatus;

            if (publicationStatus == PublicationStatus.SkippedZeroPrice && entry.Status != CatalogueIndexStatus.ZeroPriced)
            {
                await _catalogueIndexRepository.UpdateStatusAsync(entry.Id, CatalogueIndexStatus.ZeroPriced, cancellationToken);
                _logger.LogWarning(
                    "Album {BandName} - {AlbumTitle} has zero price, marking as ZeroPriced.",
                    entry.BandName,
                    entry.AlbumTitle);
            }

            await _catalogueIndexDetailRepository.UpdateAsync(existingDetail, cancellationToken);
            return (ChangeType.Updated, publicationStatus == PublicationStatus.SkippedZeroPrice);
        }

        var activePublicationStatus = DeterminePublicationStatus(existingDetail.Price, entry.Status);
        existingDetail.ChangeType = ChangeType.Active;
        existingDetail.PublicationStatus = activePublicationStatus;
        await _catalogueIndexDetailRepository.UpdateAsync(existingDetail, cancellationToken);
        return (ChangeType.Active, activePublicationStatus == PublicationStatus.SkippedZeroPrice);
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

    private static void MapAlbumFieldsToDetail(CatalogueIndexDetailEntity detail, AlbumParsedEvent source, CatalogueIndexEntity entry)
    {
        detail.DistributorCode = source.DistributorCode;
        detail.BandName = entry.BandName;
        detail.SKU = AlbumParsingHelper.TruncateSku(source.SKU);
        detail.Name = AlbumParsingHelper.TruncateName(entry.AlbumTitle);
        detail.Genre = AlbumParsingHelper.TruncateGenre(source.Genre);
        detail.Price = source.Price;
        detail.PurchaseUrl = source.PurchaseUrl;
        detail.PhotoUrl = source.PhotoUrl;
        detail.Media = source.Media;
        detail.Label = AlbumParsingHelper.TruncateLabel(source.Label);
        detail.Press = AlbumParsingHelper.TruncatePress(source.Press);
        detail.Description = source.Description;
        detail.Status = source.Status;
        detail.StockStatus = source.StockStatus;
        detail.CanonicalTitle = source.CanonicalTitle;
        detail.OriginalYear = source.OriginalYear;
        detail.MetalArchivesUrl = BuildMetalArchivesUrl(entry.BandReference);
    }

    private static ChangeReason? DetectChangeReason(CatalogueIndexDetailEntity existing, AlbumParsedEvent parsed)
    {
        bool priceChanged = existing.Price != parsed.Price;
        bool statusChanged = existing.StockStatus != parsed.StockStatus;

        if (priceChanged && statusChanged)
        {
            return ChangeReason.PriceAndStatusChange;
        }

        if (priceChanged)
        {
            return ChangeReason.PriceChange;
        }

        if (statusChanged)
        {
            return ChangeReason.StatusChange;
        }

        return null;
    }

    private async Task ProcessAlbumImageAsync(AlbumParsedEvent albumParsedEvent, IAlbumDetailParser parser, CancellationToken cancellationToken)
    {
        var imageUploadRequest = new ImageUploadRequest
        {
            ImageUrl = albumParsedEvent.PhotoUrl,
            AlbumSku = albumParsedEvent.SKU ?? Guid.NewGuid().ToString(),
            DistributorCode = albumParsedEvent.DistributorCode,
            AlbumName = albumParsedEvent.Name,
            BandName = albumParsedEvent.BandName
        };

        if (parser is BaseDistributorParser baseParser)
        {
            var (cookieHeader, userAgent) = baseParser.GetFlareSolverrCredentials();
            imageUploadRequest.CookieHeader = cookieHeader;
            imageUploadRequest.UserAgent = userAgent;
        }

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

    private static PublicationStatus DeterminePublicationStatus(float price, CatalogueIndexStatus entryStatus)
    {
        if (price <= 0)
        {
            return PublicationStatus.SkippedZeroPrice;
        }

        if (entryStatus == CatalogueIndexStatus.ZeroPriced)
        {
            return PublicationStatus.SkippedZeroPrice;
        }

        return PublicationStatus.Unpublished;
    }

    private static string? BuildMetalArchivesUrl(BandReferenceEntity? bandReference)
    {
        if (bandReference == null)
        {
            return null;
        }

        var encodedName = Uri.EscapeDataString(bandReference.BandName);
        return $"https://www.metal-archives.com/bands/{encodedName}/{bandReference.MetalArchivesId}";
    }
}
