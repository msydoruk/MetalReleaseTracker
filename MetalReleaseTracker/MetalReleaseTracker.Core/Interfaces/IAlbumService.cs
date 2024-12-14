using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IAlbumService
    {
        Task<Album> GetAlbumById(Guid id);

        Task<IEnumerable<Album>> GetAlbumsByDistributorId(Guid distributorId);

        Task<IEnumerable<Album>> GetAllAlbums();

        Task AddAlbum(Album album);

        Task<bool> UpdateAlbum(Album album);

        Task UpdateAlbumsStatus(IEnumerable<Guid> albumsIds, AlbumStatus status);

        Task UpdateAlbumPricesAndStatuses(Dictionary<Guid, (float? newPrice, AlbumStatus? newStatus)> albumPricesAndStatuses);

        Task<bool> DeleteAlbum(Guid id);

        Task<AlbumFilterResult> GetAlbumsByFilter(AlbumFilter filter);
    }
}
