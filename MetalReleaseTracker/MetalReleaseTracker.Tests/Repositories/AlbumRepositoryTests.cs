using AutoMapper;

using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;
using MetalReleaseTracker.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Tests.Repositories
{
    public class AlbumRepositoryTests : IAsyncLifetime
    {
        private MetalReleaseTrackerDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly AlbumRepository _repository;

        public AlbumRepositoryTests()
        {
            var configuration = new MapperConfiguration(configuration =>
            {
                configuration.AddProfile<MappingProfile>();
            });

            _dbContext = TestDbContextFactory.CreateDbContext();
            _mapper = configuration.CreateMapper();
            _repository = new AlbumRepository(_dbContext, _mapper);
        }

        public async Task InitializeAsync()
        {
            await InitializeData(_dbContext);
        }

        public Task DisposeAsync()
        {
            TestDbContextFactory.ClearDatabase(_dbContext);

            return Task.CompletedTask;
        }

        protected async Task InitializeData(MetalReleaseTrackerDbContext context)
        {
            var band = new BandEntity { Id = Guid.NewGuid(), Name = "Metallica" };
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
                    ReleaseDate = DateTime.SpecifyKind(new DateTime(1986, 3, 3), DateTimeKind.Utc),
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
                    ReleaseDate = DateTime.SpecifyKind(new DateTime(1984, 7, 27), DateTimeKind.Utc),
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
        public async Task GetByFilter_ShouldReturnAlbums_WhenFilterMatches()
        {
            var filter = new AlbumFilter
            {
                AlbumName = "Metallica"
            };

            var (albums, totalCount) = await _repository.GetByFilter(filter);

            Assert.NotNull(albums);
            Assert.Equal(2, albums.Count());
        }

        [Fact]
        public async Task GetByFilter_ShouldReturnEmpty_WhenInvalidFilterIsProvided()
        {
            var filter = new AlbumFilter
            {
                AlbumName = ""
            };

            var result = await _repository.GetByFilter(filter);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllAlbums()
        {
            var result = await _repository.GetAll();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetById_ShouldReturnAlbum_WhenIdExists()
        {
            var (albums, totalCount) = await _repository.GetByFilter(new AlbumFilter { AlbumName = "Metallica" });
            var albumId = albums.First().Id;

            var result = await _repository.GetById(albumId);

            Assert.NotNull(result);
            Assert.Equal(albumId, result.Id);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var result = await _repository.GetById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Add_ShouldAddAlbum()
        {
            var band = await _dbContext.Bands.FirstAsync();
            var distributor = await _dbContext.Distributors.FirstAsync();

            _dbContext.Entry(band).State = EntityState.Detached;
            _dbContext.Entry(distributor).State = EntityState.Detached;

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

            await _repository.Add(album);

            var result = await _dbContext.Albums.FindAsync(album.Id);

            Assert.NotNull(result);
            Assert.Equal(album.Name, result.Name);
        }

        [Fact]
        public async Task Add_ShouldNotAddAlbum_WhenAlbumIsInvalid()
        {
            var band = new BandEntity { Name = "Test Band" };
            await _dbContext.Bands.AddAsync(band);
            await _dbContext.SaveChangesAsync();

            var distributor = new DistributorEntity { Name = "Test Distributor", ParsingUrl = "https://example.com/universal" };
            await _dbContext.Distributors.AddAsync(distributor);
            await _dbContext.SaveChangesAsync();

            var album = new Album
            {
                Name = "",
                BandId = band.Id,
                DistributorId = distributor.Id,
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

            await _repository.Add(album);

            var result = await _dbContext.Albums.FindAsync(album.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task Update_ShouldUpdateAlbum()
        {
            var existingAlbum = await _dbContext.Albums.FirstAsync();
            var updatedAlbum = _mapper.Map<Album>(existingAlbum);

            updatedAlbum.Name = "Updated Album Name";

            var result = await _repository.Update(updatedAlbum);

            var retrievedAlbum = await _dbContext.Albums.FindAsync(updatedAlbum.Id);

            Assert.True(result);
            Assert.NotNull(retrievedAlbum);
            Assert.Equal(updatedAlbum.Name, retrievedAlbum.Name);
        }

        [Fact]
        public async Task Update_ShouldReturnFalse_WhenAlbumDoesNotExist()
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
                SKU = "DEF-456"
            };

            var result = await _repository.Update(album);

            Assert.False(result);
        }

        [Fact]
        public async Task Delete_ShouldRemoveAlbum()
        {
            var albumEntity = _dbContext.Albums.First();
            var result = await _repository.Delete(albumEntity.Id);

            var deletedAlbum = await _dbContext.Albums.FindAsync(albumEntity.Id);

            Assert.True(result);
            Assert.Null(deletedAlbum);
        }

        [Fact]
        public async Task Delete_ShouldReturnFalse_WhenAlbumDoesNotExist()
        {
            var result = await _repository.Delete(Guid.NewGuid());

            Assert.False(result);
        }
    }
}
