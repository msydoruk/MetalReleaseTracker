using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Validators;
using MetalReleaseTracker.Сore.Services;
using Moq;

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
        public async Task GetAlbumById_ShouldReturnAlbum_WhenIdExists()
        {
            var albumId = Guid.NewGuid();
            var album = new Album { Id = albumId, Name = "Test Album" };
            _albumRepository.Setup(repo => repo.GetById(albumId)).ReturnsAsync(album);

            var result = await _albumService.GetAlbumById(albumId);

            Assert.NotNull(result);
            Assert.Equal(albumId, result.Id);
            _albumRepository.Verify(repo => repo.GetById(albumId), Times.Once);
        }

        [Fact]
        public async Task GetAlbumById_ShouldThrowEntityNotFoundException_WhenIdDoesNotExist()
        {
            var albumId = Guid.NewGuid();
            _albumRepository.Setup(repo => repo.GetById(albumId)).ReturnsAsync((Album)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _albumService.GetAlbumById(albumId));
            _albumRepository.Verify(repo => repo.GetById(albumId), Times.Once);
        }

        [Fact]
        public async Task GetAllAlbums_ShouldReturnAllAlbums()
        {
            var albums = new List<Album>
            {
                new Album 
                { 
                    Id = Guid.NewGuid(),
                    DistributorId = Guid.NewGuid(),
                    BandId = Guid.NewGuid(),
                    SKU = "SKU-001",
                    Name = "Master of Puppets",
                    ReleaseDate = new DateTime(1986, 3, 3),
                    Genre = "Thrash Metal",
                    Price = 10,
                    PurchaseUrl = "http://example.com/purchase/master-of-puppets",
                    PhotoUrl = "http://example.com/photo/master-of-puppets.jpg",
                    Media = MediaType.CD,
                    Label = "Elektra",
                    Press = "First Press",
                    Description = "One of the most influential metal albums of all time.",
                    Status = AlbumStatus.Restock
                },

                new Album 
                { 
                    Id = Guid.NewGuid(),
                    DistributorId = Guid.NewGuid(),
                    BandId = Guid.NewGuid(),
                    SKU = "SKU-002",
                    Name = "Ride the Lightning",
                    ReleaseDate = new DateTime(1984, 7, 27),
                    Genre = "Thrash Metal",
                    Price = 12,
                    PurchaseUrl = "http://example.com/purchase/ride-the-lightning",
                    PhotoUrl = "http://example.com/photo/ride-the-lightning.jpg",
                    Media = MediaType.LP,
                    Label = "Elektra",
                    Press = "First Press",
                    Description = "Metallica's second album featuring some of their classic hits.",
                    Status = AlbumStatus.Preorder
                }
            };

            _albumRepository.Setup(repo => repo.GetAll()).ReturnsAsync(albums);

            var result = await _albumService.GetAllAlbums();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            _albumRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task AddAlbum_ShouldAddAlbum()
        {
            var newAlbum = new Album
            {
                Id = Guid.NewGuid(),
                DistributorId = Guid.NewGuid(),  
                BandId = Guid.NewGuid(),
                Name = "New Album",
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

            _albumRepository.Setup(repo => repo.Add(newAlbum)).Returns(Task.CompletedTask);

            await _albumService.AddAlbum(newAlbum);

            _albumRepository.Verify(repo => repo.Add(newAlbum), Times.Once);
        }

        [Fact]
        public async Task AddAlbum_ShouldThrowValidationException_WhenAlbumIsInvalid()
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
            _albumRepository.Verify(repo => repo.Add(It.IsAny<Album>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAlbum_ShouldUpdateAlbum()
        {
            var existingAlbum = new Album
            {
                Id = Guid.NewGuid(),
                BandId = Guid.NewGuid(),
                DistributorId = Guid.NewGuid(),
                Name = "Existing Album",
                ReleaseDate = DateTime.UtcNow,
                Genre = "Heavy Metal",
                Price = 12,
                SKU = "ABC-123",
                PurchaseUrl = "https://example.com/purchase",
                PhotoUrl = "https://example.com/photo",
                Label = "Label Name",
                Press = "Press Information",
                Description = "Album description"
            };

            var updatedAlbum = new Album
            {
                Id = existingAlbum.Id,
                Name = "Updated Album Name",
                BandId = existingAlbum.BandId,
                DistributorId = existingAlbum.DistributorId,
                ReleaseDate = existingAlbum.ReleaseDate,
                Genre = existingAlbum.Genre,
                Price = existingAlbum.Price,
                SKU = existingAlbum.SKU,
                PurchaseUrl = existingAlbum.PurchaseUrl,
                PhotoUrl = existingAlbum.PhotoUrl,
                Label = existingAlbum.Label,
                Press = existingAlbum.Press,
                Description = existingAlbum.Description
            };

            _albumRepository.Setup(repo => repo.GetById(existingAlbum.Id)).ReturnsAsync(existingAlbum);
            _albumRepository.Setup(repo => repo.Update(updatedAlbum)).ReturnsAsync(true);

            var result = await _albumService.UpdateAlbum(updatedAlbum);

            Assert.True(result);
            _albumRepository.Verify(repo => repo.GetById(existingAlbum.Id), Times.Once);
            _albumRepository.Verify(repo => repo.Update(updatedAlbum), Times.Once);
        }

        [Fact]
        public async Task UpdateAlbum_ShouldThrowEntityNotFoundException_WhenAlbumDoesNotExist()
        {
            var album = new Album
            {
                Id = Guid.NewGuid(),
                BandId = Guid.NewGuid(),
                DistributorId = Guid.NewGuid(),
                Name = "Non-existent Album",
                ReleaseDate = DateTime.UtcNow,
                Genre = "Heavy Metal",
                Price = 15,
                Status = AlbumStatus.New,
                SKU = "DEF-456",
                PurchaseUrl = "https://example.com/purchase",
                PhotoUrl = "https://example.com/photo",
                Media = MediaType.CD,
                Label = "Label Name",
                Press = "Press Information",
                Description = "Album description"
            };

            _albumRepository.Setup(repo => repo.GetById(album.Id)).ReturnsAsync((Album)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _albumService.UpdateAlbum(album));
            _albumRepository.Verify(repo => repo.GetById(album.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAlbum_ShouldRemoveAlbum()
        {
            var existingAlbum = new Album 
            { 
                Id = Guid.NewGuid(),
                BandId = Guid.NewGuid(),
                DistributorId = Guid.NewGuid(),
                Name = "Existing Album",
                ReleaseDate = DateTime.UtcNow,
                Genre = "Heavy Metal",
                Price = 15,
                Status = AlbumStatus.New,
                SKU = "DEF-456",
                PurchaseUrl = "https://example.com/purchase",
                PhotoUrl = "https://example.com/photo",
                Media = MediaType.CD,
                Label = "Label Name",
                Press = "Press Information",
                Description = "Album description"
            };

            _albumRepository.Setup(repo => repo.GetById(existingAlbum.Id)).ReturnsAsync(existingAlbum);
            _albumRepository.Setup(repo => repo.Delete(existingAlbum.Id)).ReturnsAsync(true); 

            var result = await _albumService.DeleteAlbum(existingAlbum.Id);

            Assert.True(result);
            _albumRepository.Verify(repo => repo.GetById(existingAlbum.Id), Times.Once);
            _albumRepository.Verify(repo => repo.Delete(existingAlbum.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAlbum_ShouldThrowEntityNotFoundException_WhenAlbumDoesNotExist()
        {
            var albumId = Guid.NewGuid();
            _albumRepository.Setup(repo => repo.GetById(albumId)).ReturnsAsync((Album)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _albumService.DeleteAlbum(albumId));
            _albumRepository.Verify(repo => repo.GetById(albumId), Times.Once);
            _albumRepository.Verify(repo => repo.Delete(albumId), Times.Never);
        }

        [Fact]
        public async Task GetAlbumsByFilter_ShouldReturnAlbums_WhenFilterMatches()
        {
            var filter = new AlbumFilter
            {
                BandName = "Metallica",
                ReleaseDateStart = new DateTime(1984, 1, 1),
                ReleaseDateEnd = new DateTime(1987, 1, 1),
                Genre = "Thrash Metal"
            };

            var albums = new List<Album>
            {
                new Album 
                { 
                    Id = Guid.NewGuid(),
                    Band = new Band { Name = "Metallica" },
                    DistributorId = Guid.NewGuid(),
                    Name = "Album 1",
                    ReleaseDate = new DateTime(1986, 3, 3),
                    Genre = "Thrash Metal",
                    Price = 15,
                    Status = AlbumStatus.New,
                    SKU = "DEF-456",
                    PurchaseUrl = "https://example.com/purchase",
                    PhotoUrl = "https://example.com/photo",
                    Media = MediaType.CD,
                    Label = "Label Name",
                    Press = "Press Information",
                    Description = "Album description"
                },

                new Album 
                { 
                    Id = Guid.NewGuid(),
                    Band = new Band { Name = "Metallica" },
                    DistributorId = Guid.NewGuid(),
                    Name = "Album 2",
                    ReleaseDate = new DateTime(1984, 7, 27),
                    Genre = "Thrash Metal",
                    Price = 15,
                    Status = AlbumStatus.New,
                    SKU = "DEF-456",
                    PurchaseUrl = "https://example.com/purchase",
                    PhotoUrl = "https://example.com/photo",
                    Media = MediaType.CD,
                    Label = "Label Name",
                    Press = "Press Information",
                    Description = "Album description"
                }
            };

            _albumRepository.Setup(repo => repo.GetByFilter(filter)).ReturnsAsync(albums);

            var result = await _albumService.GetAlbumsByFilter(filter);

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            _albumRepository.Verify(repo => repo.GetByFilter(filter), Times.Once);
        }

        [Fact]
        public async Task GetAlbumsByFilter_ShouldReturnEmpty_WhenInvalidFilterIsProvided()
        {
            var filter = new AlbumFilter
            {
                BandName = "InvalidBandName",
                ReleaseDateStart = new DateTime(1800, 1, 1),
                ReleaseDateEnd = new DateTime(1801, 1, 1),
                Genre = "InvalidGenre"
            };

            _albumRepository.Setup(repo => repo.GetByFilter(filter)).ReturnsAsync(new List<Album>());
            
            var result = await _albumService.GetAlbumsByFilter(filter);

            Assert.Empty(result);
            _albumRepository.Verify(repo => repo.GetByFilter(filter), Times.Once);
        }
    }
}
