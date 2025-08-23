using AutoMapper;
using MassTransit;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Events;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Extensions;

namespace MetalReleaseTracker.CoreDataService.Consumers;

public class AlbumProcessedEventConsumer : IConsumer<AlbumProcessedPublicationEvent>
{
    private IAlbumRepository _albumRepository;
    private IBandRepository _bandRepository;
    private IDistributorsRepository _distributorsRepository;
    private ILogger<AlbumProcessedEventConsumer> _logger;
    private IMapper _mapper;

    public AlbumProcessedEventConsumer(
        IAlbumRepository albumRepository,
        IBandRepository bandRepository,
        IDistributorsRepository distributorsRepository,
        ILogger<AlbumProcessedEventConsumer> logger,
        IMapper mapper)
    {
        _albumRepository = albumRepository;
        _bandRepository = bandRepository;
        _distributorsRepository = distributorsRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AlbumProcessedPublicationEvent> context)
    {
        try
        {
            var albumSyncedPublicationEvent = context.Message;
            var bandId = await _bandRepository.GetOrAddAsync(albumSyncedPublicationEvent.BandName);
            string distributorName = albumSyncedPublicationEvent.DistributorCode.TryGetDisplayName();

            if (string.IsNullOrEmpty(distributorName))
            {
                _logger.LogWarning(
                    $"Distributor name not mapped for code: {albumSyncedPublicationEvent.DistributorCode}.");
            }

            var distributorId = await _distributorsRepository.GetOrAddAsync(distributorName);
            var albumEntity = _mapper.Map<AlbumProcessedPublicationEvent, AlbumEntity>(albumSyncedPublicationEvent);

            albumEntity.BandId = bandId;
            albumEntity.DistributorId = distributorId;

            if (albumSyncedPublicationEvent.ProcessedStatus == AlbumProcessedStatus.New)
            {
                await _albumRepository.AddAsync(albumEntity);
                _logger.LogInformation($"Album {albumSyncedPublicationEvent.Name} was processed.");
            }
            else if (albumSyncedPublicationEvent.ProcessedStatus == AlbumProcessedStatus.Updated)
            {
                await _albumRepository.UpdateAsync(albumEntity);
                _logger.LogInformation($"Album {albumSyncedPublicationEvent.Name} was processed.");
            }
            else if (albumSyncedPublicationEvent.ProcessedStatus == AlbumProcessedStatus.Deleted)
            {
                await _albumRepository.DeleteAsync(albumSyncedPublicationEvent.Id);
                _logger.LogInformation($"Album {albumSyncedPublicationEvent.Name} was deleted.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error occurred while consuming processed albums.");
            throw;
        }
    }
}