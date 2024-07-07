﻿using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Services
{
    public interface IAlbumService
    {
        Task<Album> GetById(Guid id);

        Task<IEnumerable<Album>> GetAll();

        Task Add(Album album);

        Task<bool> Update(Album album);

        Task<bool> Delete(Guid id);

        Task<IEnumerable<Album>> GetByFilter(AlbumFilter filter);
    }
}
