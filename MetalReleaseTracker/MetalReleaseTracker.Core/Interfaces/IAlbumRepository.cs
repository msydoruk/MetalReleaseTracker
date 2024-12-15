using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IAlbumRepository
    {
        Task<Album> GetById(Guid id);

        Task<IEnumerable<Album>> GetByDistributorId(Guid distributorId);

        Task<IEnumerable<Album>> GetAll();

        Task Add(Album album);

        Task<bool> Update(Album album);

        Task<bool> UpdateAlbumsStatus(IEnumerable<Guid> albumsIds, AlbumStatus status);

        Task<bool> UpdateAlbumPricesAndStatuses(Dictionary<Guid, (float? newPrice, AlbumStatus? newStatus)> albumPricesAndStatuses);

        Task<bool> Delete(Guid id);

        Task<AlbumFilterResult> GetByFilter(AlbumFilter filter);
    }
}
