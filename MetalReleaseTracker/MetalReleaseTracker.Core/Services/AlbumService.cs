using FluentValidation;
using FluentValidation.Results;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Сore.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IValidator<Album> _albumValidator;

        public AlbumService(IAlbumRepository albumRepository, IValidator<Album> albumValidator)
        {
            _albumRepository = albumRepository;
            _albumValidator = albumValidator;
        }

        public async Task<Album> GetById(Guid id)
        {
            var album = await _albumRepository.GetById(id);
            if (album == null)
            {
                throw new EntityNotFoundException($"Album with ID {id} not found.");
            }

            return album;
        }

        public async Task<IEnumerable<Album>> GetAll()
        {
            return await _albumRepository.GetAll();
        }

        public async Task Add(Album album)
        {
            ValidateAlbum(album);

            await _albumRepository.Add(album);
        }

        public async Task<bool> Update(Album album)
        {
            ValidateAlbum(album);

            var existingAlbum = await _albumRepository.GetById(album.Id);
            if (existingAlbum == null)
            {
                throw new EntityNotFoundException($"Album with ID {album.Id} not found.");
            }

            return await _albumRepository.Update(album);
        }

        public async Task<bool> Delete(Guid id)
        {
            var album = await _albumRepository.GetById(id);
            if (album == null)
            {
                throw new EntityNotFoundException($"Album with ID {id} not found.");
            }

            return await _albumRepository.Delete(id);
        }

        public async Task<IEnumerable<Album>> GetByFilter(AlbumFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return await _albumRepository.GetByFilter(filter);
        }

        private void ValidateAlbum(Album album)
        {
            ValidationResult results = _albumValidator.Validate(album);
            if (!results.IsValid)
            {
                throw new ValidationException(results.Errors);
            }
        }
    }
}
