using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Services;

namespace MetalReleaseTracker.Application.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;

        public AlbumService(IAlbumRepository albumRepository)
        {
            _albumRepository = albumRepository;
        }

        public async Task<Album> GetById(Guid id)
        {
            return await _albumRepository.GetById(id);
        }

        public async Task<IEnumerable<Album>> GetAll()
        {
            return await _albumRepository.GetAll();
        }

        public async Task Add(Album album)
        {
            await _albumRepository.Add(album);
        }

        public async Task<bool> Update(Album album)
        {
            return await _albumRepository.Update(album);
        }

        public async Task<bool> Delete(Guid id)
        {
            return await _albumRepository.Delete(id);
        }

        public async Task<IEnumerable<Album>> GetByFilter(AlbumFilter filter)
        {
            return await _albumRepository.GetByFilter(filter);
        }
    }
}
