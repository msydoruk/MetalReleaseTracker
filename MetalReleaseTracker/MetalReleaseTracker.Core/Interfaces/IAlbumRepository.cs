using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IAlbumRepository
    {
        Task<Album> GetById(Guid id);

        Task<IEnumerable<Album>> GetAll();

        Task Add(Album album);

        Task Update(Album album);

        Task Delete(Guid id);

        Task<IEnumerable<Album>> GetByFilter(AlbumFilter filter);
    }
}
