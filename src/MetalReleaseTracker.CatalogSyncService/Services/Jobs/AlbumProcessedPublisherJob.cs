using AutoMapper;
using MassTransit;
using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MetalReleaseTracker.CatalogSyncService.Data.Events;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.CatalogSyncService.Services.Jobs;

public class AlbumProcessedPublisherJob
{
    private readonly ITopicProducer<AlbumProcessedPublicationEvent> _topicProducer;
    private readonly IAlbumProcessedRepository _albumProcessedRepository;
    private readonly ILogger<AlbumProcessedPublisherJob> _logger;
    private readonly AlbumProcessedPublisherJobSettings _processedPublisherJobSettings;
    private readonly IMapper _mapper;

    public AlbumProcessedPublisherJob(
        ITopicProducer<AlbumProcessedPublicationEvent> topicProducer,
        IAlbumProcessedRepository albumProcessedRepository,
        ILogger<AlbumProcessedPublisherJob> logger,
        IOptions<AlbumProcessedPublisherJobSettings> options,
        IMapper mapper)
    {
        _topicProducer = topicProducer;
        _albumProcessedRepository = albumProcessedRepository;
        _logger = logger;
        _mapper = mapper;
        _processedPublisherJobSettings = options.Value;
    }

    public async Task RunPublisherJob(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var albumsToPublish = await _albumProcessedRepository.GetUnPublishedBatchAsync(_processedPublisherJobSettings.BatchSize, cancellationToken);
                if (!albumsToPublish.Any())
                {
                    _logger.LogInformation("Nothing to publish");
                    break;
                }

                foreach (var albumToPublish in albumsToPublish)
                {
                    try
                    {
                        await PublishEvent(albumToPublish, cancellationToken);
                        await SetProcessedAlbumToPublishedAsync(albumToPublish, cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, $"Album publishing failed. SKU: {albumToPublish.SKU}, Distributor: {albumToPublish.DistributorCode}.");
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred while publishing albums to catalog sync topic.");
            throw;
        }
    }

    private async Task PublishEvent(AlbumProcessedEntity albumToPublish, CancellationToken cancellationToken)
    {
        var albumProcessedPublicationEvent =
            _mapper.Map<AlbumProcessedEntity, AlbumProcessedPublicationEvent>(albumToPublish);
        await _topicProducer.Produce(albumProcessedPublicationEvent, cancellationToken);

        _logger.LogInformation($"Published album to catalog sync topic. SKU: {albumToPublish.SKU}, Distributor: {albumToPublish.DistributorCode}.");
    }

    private async Task SetProcessedAlbumToPublishedAsync(AlbumProcessedEntity processedAlbum, CancellationToken cancellationToken)
    {
        var changedFields = new Dictionary<string, object>
        {
            {
                nameof(AlbumProcessedEntity.ProcessedStatus), AlbumProcessedStatus.Published
            },
            {
                nameof(AlbumProcessedEntity.LastPublishedDate), DateTime.UtcNow
            }
        };

        await _albumProcessedRepository.UpdateAsync(processedAlbum.Id, changedFields, cancellationToken);
    }
}