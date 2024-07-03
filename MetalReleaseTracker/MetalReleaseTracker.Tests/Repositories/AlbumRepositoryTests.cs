﻿using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Repositories;
using MetalReleaseTracker.Tests.Base;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Tests.Repositories
{
    public class AlbumRepositoryTests : IntegrationTestBase
    {
        private readonly AlbumRepository _repository;

        public AlbumRepositoryTests()
        {
            _repository = new AlbumRepository(DbContext, Mapper);
        }

        protected override void InitializeData(MetalReleaseTrackerDbContext context)
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
        public async Task GetByFilter_ShouldReturnAlbums_WhenFilterMatches()
        {
            var filter = new AlbumFilter
            {
                BandName = "Metallica",
                ReleaseDateStart = new DateTime(1984, 1, 1),
                ReleaseDateEnd = new DateTime(1987, 1, 1)
            };

            var result = await _repository.GetByFilter(filter);

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByFilter_ShouldReturnEmpty_WhenInvalidFilterIsProvided()
        {
            var filter = new AlbumFilter
            {
                BandName = "",
                ReleaseDateStart = new DateTime(1800, 1, 1),
                ReleaseDateEnd = new DateTime(1801, 1, 1)
            };

            var result = await _repository.GetByFilter(filter);

            Assert.Empty(result);
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
            var album = await _repository.GetByFilter(new AlbumFilter { BandName = "Metallica" });
            var albumId = album.First().Id;

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

            await _repository.Add(album);

            var result = await DbContext.Albums.FindAsync(album.Id);

            Assert.NotNull(result);
            Assert.Equal(album.Name, result.Name);
        }

        [Fact]
        public async Task Add_ShouldNotAddAlbum_WhenAlbumIsInvalid()
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

            await _repository.Add(album);

            var result = await DbContext.Albums.FindAsync(album.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task Update_ShouldUpdateAlbum()
        {
            var existingAlbum = await DbContext.Albums.FirstAsync();
            var updatedAlbum = Mapper.Map<Album>(existingAlbum);

            updatedAlbum.Name = "Updated Album Name";

            var result = await _repository.Update(updatedAlbum);

            var retrievedAlbum = await DbContext.Albums.FindAsync(updatedAlbum.Id);

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
            var albumEntity = DbContext.Albums.First();
            var result = await _repository.Delete(albumEntity.Id);

            var deletedAlbum = await DbContext.Albums.FindAsync(albumEntity.Id);

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