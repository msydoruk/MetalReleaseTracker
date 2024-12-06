using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Core.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IValidationService _validationService;

        public AlbumService(IAlbumRepository albumRepository, IValidationService validationService)
        {
            _albumRepository = albumRepository;
            _validationService = validationService;
        }

        public async Task<Album> GetAlbumById(Guid id)
        {
            _validationService.Validate(id);

            return await EnsureAlbumExists(id);
        }

        public async Task<IEnumerable<Album>> GetAlbumsByDistributorId(Guid distributorId)
        {
            _validationService.Validate(distributorId);

            return await _albumRepository.GetByDistributorId(distributorId);
        }

        public async Task<IEnumerable<Album>> GetAllAlbums()
        {
            return await _albumRepository.GetAll();
        }

        public async Task AddAlbum(Album album)
        {
            _validationService.Validate(album);

            await _albumRepository.Add(album);
        }

        public async Task<bool> UpdateAlbum(Album album)
        {
            _validationService.Validate(album);

            await EnsureAlbumExists(album.Id);

            return await _albumRepository.Update(album);
        }

        public async Task UpdateAlbumsStatus(IEnumerable<Guid> albumsIds, AlbumStatus status)
        {
            foreach (var albumId in albumsIds)
            {
                _validationService.Validate(albumId);
            }

            await _albumRepository.UpdateAlbumsStatus(albumsIds, status);
        }

        public async Task UpdateAlbumPrices(Dictionary<Guid, float> albumPrices)
        {
            foreach (var albumId in albumPrices.Keys)
            {
                _validationService.Validate(albumId);
            }

            await _albumRepository.UpdateAlbumPrices(albumPrices);
        }

        public async Task<bool> DeleteAlbum(Guid id)
        {
            _validationService.Validate(id);

            await EnsureAlbumExists(id);

            return await _albumRepository.Delete(id);
        }

        public async Task<(IEnumerable<Album>, int)> GetAlbumsByFilter(AlbumFilter filter)
        {
            _validationService.Validate(filter);

            return await _albumRepository.GetByFilter(filter);
        }

        private async Task<Album> EnsureAlbumExists(Guid id)
        {
            var album = await _albumRepository.GetById(id);
            if (album == null)
            {
                throw new EntityNotFoundException($"Album with ID {id} not found.");
            }

            return album;
        }
    }
}
