using AutoMapper;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Events;
using MetalReleaseTracker.CoreDataService.Services.Dtos;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Data.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AlbumEntity, AlbumDto>()
            .ForMember(destination => destination.BandName, options => options.MapFrom(source => source.Band.Name))
            .ForMember(destination => destination.BandSlug, options => options.MapFrom(source => source.Band.Slug))
            .ForMember(destination => destination.DistributorName,
                options => options.MapFrom(source => source.Distributor.Name));

        CreateMap<AlbumProcessedPublicationEvent, AlbumEntity>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Slug, options => options.Ignore())
            .ForMember(destination => destination.CreatedDate, options => options.MapFrom(source => source.CreatedDate))
            .ForMember(destination => destination.LastUpdateDate,
                options => options.MapFrom(source => source.LastUpdateDate));

        CreateMap<BandEntity, BandDto>();

        CreateMap<DistributorEntity, DistributorDto>();

        CreateMap<AlbumChangeLogEntity, AlbumChangeLogDto>();
    }
}