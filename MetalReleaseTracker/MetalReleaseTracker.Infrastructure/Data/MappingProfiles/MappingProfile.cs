﻿using AutoMapper;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Infrastructure.Data.Entities;

namespace MetalReleaseTracker.Infrastructure.Data.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Album, AlbumEntity>().ReverseMap();
            CreateMap<Band, BandEntity>().ReverseMap();
            CreateMap<Distributor, DistributorEntity>().ReverseMap();
            CreateMap<Subscription, SubscriptionEntity>().ReverseMap();
        }
    }
}
