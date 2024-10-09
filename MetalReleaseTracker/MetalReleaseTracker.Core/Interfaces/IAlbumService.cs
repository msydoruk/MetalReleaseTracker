using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IAlbumService
    {
        Task<Album> GetAlbumById(Guid id);

        Task<IEnumerable<Album>> GetAllAlbums();

        Task AddAlbum(Album album);

        Task<bool> UpdateAlbum(Album album);

        Task UpdateAlbumsStatus(IEnumerable<Guid> albumsIds);

        Task UpdatePriceForAlbums(IEnumerable<Guid> albumIds, float newPrice);

        Task<bool> DeleteAlbum(Guid id);

        Task<IEnumerable<Album>> GetAlbumsByFilter(AlbumFilter filter);

        Task<IEnumerable<Album>> GetAlbumsByDistributor(Guid distributorId);
    }
}
