using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IAlbumRepository
    {
        Task<Album> GetById(Guid id);

        Task<IEnumerable<Album>> GetAll();

        Task Add(Album album);

        Task<bool> Update(Album album);

        Task<bool> UpdateAlbums(IEnumerable<Album> albums);

        Task<bool> Delete(Guid id);

        Task<IEnumerable<Album>> GetByFilter(AlbumFilter filter);

        Task<IEnumerable<Album>> GetByDistributorId(Guid distributorId);
    }
}
