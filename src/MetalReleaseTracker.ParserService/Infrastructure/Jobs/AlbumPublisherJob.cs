using MassTransit;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class AlbumPublisherJob
{
    private const int BatchSize = 100;

    private readonly ICatalogueIndexDetailRepository _catalogueIndexDetailRepository;
    private readonly ITopicProducer<AlbumProcessedPublicationEvent> _topicProducer;
    private readonly IParsingProgressTracker _progressTracker;
    private readonly ILogger<AlbumPublisherJob> _logger;

    public AlbumPublisherJob(
        ICatalogueIndexDetailRepository catalogueIndexDetailRepository,
        ITopicProducer<AlbumProcessedPublicationEvent> topicProducer,
        IParsingProgressTracker progressTracker,
        ILogger<AlbumPublisherJob> logger)
    {
        _catalogueIndexDetailRepository = catalogueIndexDetailRepository;
        _topicProducer = topicProducer;
        _progressTracker = progressTracker;
        _logger = logger;
    }

    public async Task RunPublisherJob(CancellationToken cancellationToken)
    {
        var trackingStarted = false;
        var runId = Guid.NewGuid();

        try
        {
            var totalPublished = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                var unpublished = await _catalogueIndexDetailRepository.GetUnpublishedAsync(BatchSize, cancellationToken);

                if (unpublished.Count == 0)
                {
                    break;
                }

                if (!trackingStarted)
                {
                    _progressTracker.StartRun(runId, ParsingJobType.AlbumPublisher, unpublished.Count);
                    trackingStarted = true;
                }

                _logger.LogInformation("Publishing batch of {Count} album changes.", unpublished.Count);

                foreach (var detail in unpublished)
                {
                    if (string.IsNullOrWhiteSpace(detail.CanonicalTitle))
                    {
                        _logger.LogWarning(
                            "Skipping album without CanonicalTitle: '{BandName}' - '{AlbumName}' (SKU={SKU}).",
                            detail.BandName,
                            detail.Name,
                            detail.SKU);
                        detail.PublicationStatus = PublicationStatus.SkippedNoCanonicalTitle;
                        await _catalogueIndexDetailRepository.UpdateAsync(detail, cancellationToken);
                        continue;
                    }

                    var publicationEvent = MapToPublicationEvent(detail);
                    await _topicProducer.Produce(publicationEvent, cancellationToken);

                    detail.PublicationStatus = PublicationStatus.Published;
                    detail.LastPublishedAt = DateTime.UtcNow;
                    await _catalogueIndexDetailRepository.UpdateAsync(detail, cancellationToken);

                    var category = MapChangeTypeToCategory(detail.ChangeType);
                    _progressTracker.ItemProcessed(runId, $"{detail.BandName} - {detail.Name}", category);
                }

                totalPublished += unpublished.Count;
            }

            if (totalPublished == 0)
            {
                _logger.LogInformation("No unpublished album changes to publish.");
            }
            else
            {
                _progressTracker.CompleteRun(runId);
                _logger.LogInformation("Published {Count} album changes to albums-processed-topic.", totalPublished);
            }
        }
        catch (Exception exception)
        {
            if (trackingStarted)
            {
                _progressTracker.FailRun(runId, exception.Message);
            }

            _logger.LogError(exception, "Error occurred while publishing album changes.");
            throw;
        }
    }

    private static AlbumProcessedPublicationEvent MapToPublicationEvent(CatalogueIndexDetailEntity detail)
    {
        return new AlbumProcessedPublicationEvent
        {
            Id = detail.CatalogueIndexId,
            ProcessedStatus = MapChangeType(detail.ChangeType),
            CreatedDate = detail.CreatedAt,
            LastUpdateDate = detail.UpdatedAt,
            LastCheckedDate = DateTime.UtcNow,
            LastPublishedDate = DateTime.UtcNow,
            DistributorCode = detail.DistributorCode,
            BandName = detail.BandName,
            SKU = detail.SKU,
            Name = detail.Name,
            Genre = detail.Genre,
            Price = detail.Price,
            PurchaseUrl = detail.PurchaseUrl,
            PhotoUrl = detail.PhotoUrl,
            Media = detail.Media,
            Label = detail.Label,
            Press = detail.Press,
            Description = detail.Description,
            Status = detail.Status,
            StockStatus = detail.StockStatus,
            CanonicalTitle = detail.CanonicalTitle,
            OriginalYear = detail.OriginalYear,
            MetalArchivesUrl = detail.MetalArchivesUrl,
            BandcampUrl = detail.BandcampUrl,
        };
    }

    private static AlbumProcessedStatus MapChangeType(ChangeType changeType)
    {
        return changeType switch
        {
            ChangeType.New => AlbumProcessedStatus.New,
            ChangeType.Updated => AlbumProcessedStatus.Updated,
            ChangeType.Deleted => AlbumProcessedStatus.Deleted,
            _ => throw new ArgumentOutOfRangeException(nameof(changeType), changeType, "Unexpected ChangeType for publishing."),
        };
    }

    private static string MapChangeTypeToCategory(ChangeType changeType)
    {
        return changeType switch
        {
            ChangeType.New => "new",
            ChangeType.Updated => "updated",
            ChangeType.Deleted => "deleted",
            _ => "new",
        };
    }
}
