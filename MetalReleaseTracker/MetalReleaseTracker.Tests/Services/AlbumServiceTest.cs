using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Validators;
using MetalReleaseTracker.Core.Services;
using Moq;
using Xunit.Abstractions;

namespace MetalReleaseTracker.Tests.Services
{
    public class AlbumServiceTest
    {
        private readonly Mock<IAlbumRepository> _albumRepository;
        private readonly ValidationService _validationService;
        private readonly AlbumService _albumService;

        public AlbumServiceTest()
        {
            _albumRepository = new Mock<IAlbumRepository>();
            _validationService = new ValidationService(new List<IValidator>
            {
                new AlbumValidator(),
                new AlbumFilterValidator(),
                new GuidValidator()
            });
            _albumService = new AlbumService(_albumRepository.Object, _validationService);
        }

        [Fact]
        public async Task GetAlbumById_WhenIdExists_ShouldReturnAlbum()
        {
            var albumId = Guid.NewGuid();
            var album = CreateSampleAlbum(albumId);
            _albumRepository.Setup(repository => repository.GetById(albumId)).ReturnsAsync(album);

            var result = await _albumService.GetAlbumById(albumId);

            Assert.NotNull(result);
            Assert.Equal(albumId, result.Id);
            _albumRepository.Verify(repository => repository.GetById(albumId), Times.Once);
        }

        [Fact]
        public async Task GetAlbumById_WhenIdDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var albumId = Guid.NewGuid();
            _albumRepository.Setup(repository => repository.GetById(albumId)).ReturnsAsync((Album)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _albumService.GetAlbumById(albumId));
            _albumRepository.Verify(repository => repository.GetById(albumId), Times.Once);
        }

        [Fact]
        public async Task GetAllAlbums_ShouldReturnAllAlbums()
        {
            var albums = CreateSampleAlbums();
            _albumRepository.Setup(repository => repository.GetAll()).ReturnsAsync(albums);

            var result = await _albumService.GetAllAlbums();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            _albumRepository.Verify(repository => repository.GetAll(), Times.Once);
        }

        [Fact]
        public async Task AddAlbum_ShouldAddAlbum()
        {
            var newAlbum = CreateSampleAlbum();
            _albumRepository.Setup(repository => repository.Add(newAlbum)).Returns(Task.CompletedTask);

            await _albumService.AddAlbum(newAlbum);

            _albumRepository.Verify(repository => repository.Add(newAlbum), Times.Once);
        }

        [Fact]
        public async Task AddAlbum_WhenAlbumIsInvalid_ShouldThrowValidationException()
        {
            var album = new Album
            {
                Name = "",
                ReleaseDate = DateTime.UtcNow,
                Genre = "",
                Price = 0,
                Status = AlbumStatus.New,
                SKU = "",
                PurchaseUrl = "",
                PhotoUrl = "",
                Media = MediaType.CD,
                Label = "",
                Press = "",
                Description = ""
            };

            await Assert.ThrowsAsync<ValidationException>(() => _albumService.AddAlbum(album));
            _albumRepository.Verify(repository => repository.Add(It.IsAny<Album>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAlbum_ShouldUpdateAlbum()
        {
            var existingAlbum = CreateSampleAlbum();
            var updatedAlbum = CreateSampleAlbum(existingAlbum.Id, existingAlbum.DistributorId, existingAlbum.BandId, "Updated Album Name");

            _albumRepository.Setup(repository => repository.GetById(existingAlbum.Id)).ReturnsAsync(existingAlbum);
            _albumRepository.Setup(repository => repository.Update(updatedAlbum)).ReturnsAsync(true);

            var result = await _albumService.UpdateAlbum(updatedAlbum);

            Assert.True(result);
            _albumRepository.Verify(repository => repository.GetById(existingAlbum.Id), Times.Once);
            _albumRepository.Verify(repository => repository.Update(updatedAlbum), Times.Once);
        }

        [Fact]
        public async Task UpdateAlbum_WhenAlbumDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var album = CreateSampleAlbum();
            _albumRepository.Setup(repository => repository.GetById(album.Id)).ReturnsAsync((Album)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _albumService.UpdateAlbum(album));
            _albumRepository.Verify(repository => repository.GetById(album.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAlbum_ShouldRemoveAlbum()
        {
            var existingAlbum = CreateSampleAlbum();
            _albumRepository.Setup(repository => repository.GetById(existingAlbum.Id)).ReturnsAsync(existingAlbum);
            _albumRepository.Setup(repository => repository.Delete(existingAlbum.Id)).ReturnsAsync(true);

            var result = await _albumService.DeleteAlbum(existingAlbum.Id);

            Assert.True(result);
            _albumRepository.Verify(repository => repository.GetById(existingAlbum.Id), Times.Once);
            _albumRepository.Verify(repository => repository.Delete(existingAlbum.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAlbum_WhenAlbumDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var albumId = Guid.NewGuid();
            _albumRepository.Setup(repository => repository.GetById(albumId)).ReturnsAsync((Album)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _albumService.DeleteAlbum(albumId));
            _albumRepository.Verify(repository => repository.GetById(albumId), Times.Once);
            _albumRepository.Verify(repository => repository.Delete(albumId), Times.Never);
        }

        [Fact]
        public async Task GetAlbumsByFilter_WhenFilterMatches_ShouldReturnAlbums()
        {
            var filter = new AlbumFilter
            {
                AlbumName = "Album 1"
            };

            var albums = new List<Album>
            {
                CreateSampleAlbum(name: "Album 1"),
                CreateSampleAlbum(name: "Album 2")
            };

            albums[0].Band = new Band { Name = "Metallica" };
            albums[1].Band = new Band { Name = "Metallica" };

            _albumRepository.Setup(repository => repository.GetByFilter(filter)).ReturnsAsync((albums, albums.Count));

            var result = (await _albumService.GetAlbumsByFilter(filter)).Item1;

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _albumRepository.Verify(repository => repository.GetByFilter(filter), Times.Once);
        }

        [Fact]
        public async Task GetAlbumsByFilter_WhenInvalidFilterIsProvided_ShouldReturnEmpty()
        {
            var filter = new AlbumFilter
            {
                AlbumName = "InvalidBandName"
            };

            _albumRepository.Setup(repository => repository.GetByFilter(filter)).ReturnsAsync((new List<Album>(), 0));


            var result = await _albumService.GetAlbumsByFilter(filter);

            Assert.Null(result);
            _albumRepository.Verify(repository => repository.GetByFilter(filter), Times.Once);
        }

        private Album CreateSampleAlbum(Guid? id = null, Guid? distributorId = null, Guid? bandId = null, string name = "Test Album")
        {
            return new Album
            {
                Id = id ?? Guid.NewGuid(),
                DistributorId = distributorId ?? Guid.NewGuid(),
                BandId = bandId ?? Guid.NewGuid(),
                Name = name,
                ReleaseDate = DateTime.UtcNow,
                Genre = "Heavy Metal",
                Price = 12,
                Status = AlbumStatus.New,
                SKU = "ABC-123",
                PurchaseUrl = "https://example.com/purchase",
                PhotoUrl = "https://example.com/photo",
                Media = MediaType.CD,
                Label = "Label Name",
                Press = "Press Information",
                Description = "Album description"
            };
        }

        private List<Album> CreateSampleAlbums()
        {
            return new List<Album>
            {
                 CreateSampleAlbum(name: "Master of Puppets"),
                CreateSampleAlbum(name: "Ride the Lightning")
            };
        }
    }
}
