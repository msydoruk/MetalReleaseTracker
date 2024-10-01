using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IAlbumService
    {
        Task<Album> GetAlbumById(Guid id);

        Task<IEnumerable<Album>> GetAllAlbums();

        Task AddAlbum(Album album);

        Task<bool> UpdateAlbum(Album album);

        Task<bool> DeleteAlbum(Guid id);

        Task<IEnumerable<Album>> GetAlbumsByFilter(AlbumFilter filter);

        Task<IEnumerable<Album>> GetAllAlbumsFromDistributor(Guid distributorId);
    }
}
