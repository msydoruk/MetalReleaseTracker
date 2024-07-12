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
        private readonly IValidator<AlbumFilter> _albumFilterValidator;

        public AlbumService(IAlbumRepository albumRepository, IValidator<Album> albumValidator, IValidator<AlbumFilter> albumFilterValidator)
        {
            _albumRepository = albumRepository;
            _albumValidator = albumValidator;
            _albumFilterValidator = albumFilterValidator;
        }

        public async Task<Album> GetById(Guid id)
        {
            ValidateGuid(id);

            return await GetExistingAlbumById(id);
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

            await GetExistingAlbumById(album.Id);

            return await _albumRepository.Update(album);
        }

        public async Task<bool> Delete(Guid id)
        {
            ValidateGuid(id);

            await GetExistingAlbumById(id);

            return await _albumRepository.Delete(id);
        }

        public async Task<IEnumerable<Album>> GetByFilter(AlbumFilter filter)
        {
            ValidateFilter(filter);

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

        private void ValidateFilter(AlbumFilter filter)
        {
            ValidationResult results = _albumFilterValidator.Validate(filter);
            if (!results.IsValid)
            {
                throw new ValidationException(results.Errors);
            }
        }

        private async Task<Album> GetExistingAlbumById(Guid id)
        {
            var album = await _albumRepository.GetById(id);
            if (album == null)
            {
                throw new EntityNotFoundException($"Album with ID {id} not found.");
            }

            return album;
        }

        private void ValidateGuid(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("The ID must be a non-empty GUID.", nameof(id));
            }
        }
    }
}
