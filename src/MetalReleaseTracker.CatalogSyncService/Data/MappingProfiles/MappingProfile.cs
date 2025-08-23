using AutoMapper;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Events;

namespace MetalReleaseTracker.CatalogSyncService.Data.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RawAlbumEntity, AlbumProcessedEntity>().ForMember(destination => destination.CreatedDate,
            option => option.MapFrom(value => DateTime.UtcNow));
        CreateMap<AlbumProcessedEntity, AlbumProcessedPublicationEvent>();
    }
}