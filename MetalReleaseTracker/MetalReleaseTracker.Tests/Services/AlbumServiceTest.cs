using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Validators;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Repositories;
using MetalReleaseTracker.Tests.Base;
using MetalReleaseTracker.Сore.Services;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Tests.Services
{
    public class AlbumServiceTest : IntegrationTestBase
    {
        private readonly AlbumRepository _albumRepository;
        private readonly ValidationService _validationService;
        private readonly AlbumService _albumService;

        public AlbumServiceTest()
        {
            _albumRepository = new AlbumRepository(DbContext, Mapper);
            _validationService = new ValidationService(new List<IValidator> 
            { 
                new AlbumValidator(),
                new AlbumFilterValidator(),
                new GuidValidator()
            });
            _albumService = new AlbumService(_albumRepository, _validationService);
        }

        protected override void InitializeData(MetalReleaseTrackerDbContext context)
        {
            var band = new BandEntity 
            { 
                Id = Guid.NewGuid(), 
                Name = "Metallica" 
            };
            context.Bands.Add(band);

            var distributor = new DistributorEntity
            {
                Id = Guid.NewGuid(),
                Name = "Universal Music",
                ParsingUrl = "http://example.com/universal"
            };
            context.Distributors.Add(distributor);

            var albums = new[]
            {
                new AlbumEntity
                {
                    Id = Guid.NewGuid(),
                    Band = band,
                    BandId = band.Id,
                    Distributor = distributor,
                    DistributorId = distributor.Id,
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
                new AlbumEntity
                {
                    Id = Guid.NewGuid(),
                    Band = band,
                    BandId = band.Id,
                    Distributor = distributor,
                    DistributorId = distributor.Id,
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
            context.Albums.AddRange(albums);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetAlbumById_ShouldReturnAlbum_WhenIdExists()
        {
            var existingAlbum = await DbContext.Albums.FirstAsync();
            var result = await _albumService.GetAlbumById(existingAlbum.Id);

            Assert.NotNull(result);
            Assert.Equal(existingAlbum.Id, result.Id);
        }

        [Fact]
        public async Task GetAlbumById_ShouldThrowEntityNotFoundException_WhenIdDoesNotExist()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _albumService.GetAlbumById(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAllAlbums_ShouldReturnAllAlbums()
        {
            var result = await _albumService.GetAllAlbums();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddAlbum_ShouldAddAlbum()
        {
            var band = await DbContext.Bands.FirstAsync();
            var distributor = await DbContext.Distributors.FirstAsync();

            DbContext.Entry(band).State = EntityState.Detached;
            DbContext.Entry(distributor).State = EntityState.Detached;

            var album = new Album
            {
                Id = Guid.NewGuid(),
                BandId = band.Id,
                DistributorId = distributor.Id,
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

            await _albumService.AddAlbum(album);

            var result = await DbContext.Albums.FindAsync(album.Id);

            Assert.NotNull(result);
            Assert.Equal(album.Name, result.Name);
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
        }

        [Fact]
        public async Task UpdateAlbum_ShouldUpdateAlbum()
        {
            var existingAlbum = await DbContext.Albums.FirstAsync();
            var updatedAlbum = Mapper.Map<Album>(existingAlbum);
            updatedAlbum.Name = "Updated Album Name";
            var result = await _albumService.UpdateAlbum(updatedAlbum);

            var retrievedAlbum = await DbContext.Albums.FindAsync(updatedAlbum.Id);

            Assert.True(result);
            Assert.NotNull(retrievedAlbum);
            Assert.Equal(updatedAlbum.Name, retrievedAlbum.Name);
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

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _albumService.UpdateAlbum(album));
        }

        [Fact]
        public async Task DeleteAlbum_ShouldRemoveAlbum()
        {
            var existingAlbum = await DbContext.Albums.FirstAsync();
            var result = await _albumService.DeleteAlbum(existingAlbum.Id);

            var deletedAlbum = await DbContext.Albums.FindAsync(existingAlbum.Id);

            Assert.True(result);
            Assert.Null(deletedAlbum);
        }

        [Fact]
        public async Task DeleteAlbum_ShouldThrowEntityNotFoundException_WhenAlbumDoesNotExist()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _albumService.DeleteAlbum(Guid.NewGuid()));
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

            var result = await _albumService.GetAlbumsByFilter(filter);

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
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

            var result = await _albumService.GetAlbumsByFilter(filter);

            Assert.Empty(result);
        }
    }
}
