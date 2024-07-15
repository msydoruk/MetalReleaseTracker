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

        public async Task<Album> GetByIdAlbum(Guid id)
        {
            ValidateGuid(id);

            return await EnsureAlbumExists(id);
        }

        public async Task<IEnumerable<Album>> GetAllAlbums()
        {
            return await _albumRepository.GetAll();
        }

        public async Task AddAlbum(Album album)
        {
            ValidateAlbum(album);

            await _albumRepository.Add(album);
        }

        public async Task<bool> UpdateAlbum(Album album)
        {
            ValidateAlbum(album);

            await EnsureAlbumExists(album.Id);

            return await _albumRepository.Update(album);
        }

        public async Task<bool> DeleteAlbum(Guid id)
        {
            ValidateGuid(id);

            await EnsureAlbumExists(id);

            return await _albumRepository.Delete(id);
        }

        public async Task<IEnumerable<Album>> GetByFilterAlbums(AlbumFilter filter)
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

        private async Task<Album> EnsureAlbumExists(Guid id)
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
