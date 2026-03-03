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
            var unpublished = await _catalogueIndexDetailRepository.GetUnpublishedAsync(BatchSize, cancellationToken);

            if (unpublished.Count == 0)
            {
                _logger.LogInformation("No unpublished album changes to publish.");
                return;
            }

            _progressTracker.StartRun(runId, ParsingJobType.AlbumPublisher, unpublished.Count);
            trackingStarted = true;

            _logger.LogInformation("Publishing {Count} album changes.", unpublished.Count);

            foreach (var detail in unpublished)
            {
                var publicationEvent = MapToPublicationEvent(detail);
                await _topicProducer.Produce(publicationEvent, cancellationToken);

                detail.PublicationStatus = PublicationStatus.Published;
                detail.LastPublishedAt = DateTime.UtcNow;
                await _catalogueIndexDetailRepository.UpdateAsync(detail, cancellationToken);

                var category = MapChangeTypeToCategory(detail.ChangeType);
                _progressTracker.ItemProcessed(runId, $"{detail.BandName} - {detail.Name}", category);
            }

            _progressTracker.CompleteRun(runId);

            _logger.LogInformation("Published {Count} album changes to albums-processed-topic.", unpublished.Count);
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
            CanonicalTitle = detail.CanonicalTitle,
            OriginalYear = detail.OriginalYear,
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
