﻿using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Application.Services;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace MetalReleaseTracker.Tests.Services
{
    public class AlbumSynchronizationServiceTests
    {
        private readonly Mock<IParserFactory> _parserFactoryMock;
        private readonly Mock<IAlbumService> _albumServiceMock;
        private readonly Mock<IBandService> _bandServiceMock;
        private readonly Mock<IDistributorsService> _distributorServiceMock;
        private readonly Mock<ILogger<AlbumSynchronizationService>> _loggerMock;
        private readonly AlbumSynchronizationService _service;
        
        public AlbumSynchronizationServiceTests()
        {
            _parserFactoryMock = new Mock<IParserFactory>();
            _albumServiceMock = new Mock<IAlbumService>();
            _bandServiceMock = new Mock<IBandService>();
            _distributorServiceMock = new Mock<IDistributorsService>();
            _loggerMock =  new Mock<ILogger<AlbumSynchronizationService>>();

            _service = new AlbumSynchronizationService(_parserFactoryMock.Object, _albumServiceMock.Object, _bandServiceMock.Object, _distributorServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SynchronizeAllAlbums_WhenNoExistingAlbums_ShouldAddNewAlbums()
        {
            var distributor = new Distributor 
            { 
                Id = Guid.NewGuid(), 
                Code = DistributorCode.OsmoseProductions, 
                ParsingUrl = "testUrl" 
            };

            var distributors = new List<Distributor> { distributor };

            var parsedAlbums = new List<AlbumDto>
            {
               CreateSampleAlbumDto()
            };
            var existingAlbums = new List<Album>();
            var band = new Band 
            { 
                Id = Guid.NewGuid(), 
                Name = "Band" 
            };

            var parserMock = new Mock<IParser>();
            parserMock.Setup(parser => parser.ParseAlbums(It.IsAny<string>())).ReturnsAsync(parsedAlbums);

            _parserFactoryMock.Setup(factory => factory.CreateParser(It.IsAny<DistributorCode>())).Returns(parserMock.Object);

            _distributorServiceMock.Setup(service => service.GetAllDistributors()).ReturnsAsync(distributors);

            _bandServiceMock.Setup(service => service.GetBandByName(It.IsAny<string>())).ReturnsAsync(band);

            _albumServiceMock.Setup(service => service.GetAlbumsByDistributorId(It.IsAny<Guid>())).ReturnsAsync(existingAlbums);

            await _service.SynchronizeAllAlbums();

            _albumServiceMock.Verify(albumService => albumService.AddAlbum(It.IsAny<Album>()), Times.Once);
        }

        [Fact]
        public async Task SynchronizeAllAlbums_WhenAlbumExists_ShouldUpdatesExistingAlbums()
        {
            var distributor = new Distributor 
            { 
                Id = Guid.NewGuid(), 
                Code = DistributorCode.OsmoseProductions, 
                ParsingUrl = "testUrl" 
            };

            var distributors = new List<Distributor> { distributor };

            var parsedAlbums = new List<AlbumDto>
            {
                CreateSampleAlbumDto()
            };
            var existingAlbums = new List<Album>
            {
                CreateSampleAlbum(sku: "SKU1")
            };

            var band = new Band 
            { 
                Id = Guid.NewGuid(), 
                Name = "Band" 
            };

            var parserMock = new Mock<IParser>();
            parserMock.Setup(parser => parser.ParseAlbums(It.IsAny<string>())).ReturnsAsync(parsedAlbums);

            _parserFactoryMock.Setup(factory => factory.CreateParser(It.IsAny<DistributorCode>())).Returns(parserMock.Object);

            _distributorServiceMock.Setup(service => service.GetAllDistributors()).ReturnsAsync(distributors);

            _bandServiceMock.Setup(band => band.GetBandByName(It.IsAny<string>())).ReturnsAsync(band);

            _albumServiceMock.Setup(album => album.GetAlbumsByDistributorId(It.IsAny<Guid>())).ReturnsAsync(existingAlbums);

            await _service.SynchronizeAllAlbums();

            var expectedAlbumPrices = new Dictionary<Guid, (float? newPrice, AlbumStatus? newStatus)>
            {
                { existingAlbums.First().Id, (12, AlbumStatus.New) }
            };

            _albumServiceMock.Verify(albumService => albumService.UpdateAlbumPricesAndStatuses(It.Is<Dictionary<Guid, (float? newPrice, AlbumStatus? newStatus)>>(prices => prices.Count == 1 && prices.ContainsKey(existingAlbums.First().Id) && prices[existingAlbums.First().Id].newPrice == 12 && prices[existingAlbums.First().Id].newStatus == AlbumStatus.New)), Times.Once);

        }

        [Fact]
        public async Task SynchronizeAllAlbums_WhenOldAlbumsExist_ShouldHidesOldAlbums()
        {
            var distributor = new Distributor 
            { 
                Id = Guid.NewGuid(), 
                Code = DistributorCode.OsmoseProductions, 
                ParsingUrl = "http://example.com" 
            };

            var distributors = new List<Distributor> { distributor };

            var parsedAlbums = new List<AlbumDto>
            {
                CreateSampleAlbumDto()
            };
            var existingAlbums = new List<Album>
            {
                CreateSampleAlbum(sku: "SKU1"),
                CreateSampleAlbum(sku: "SKU2"),
            };
            var band = new Band 
            { 
                Id = Guid.NewGuid(), 
                Name = "Band" 
            };

            var parserMock = new Mock<IParser>();
            parserMock.Setup(parser => parser.ParseAlbums(It.IsAny<string>())).ReturnsAsync(parsedAlbums);

            _parserFactoryMock.Setup(factory => factory.CreateParser(It.IsAny<DistributorCode>())).Returns(parserMock.Object);

            _distributorServiceMock.Setup(service => service.GetAllDistributors()).ReturnsAsync(distributors);

            _bandServiceMock.Setup(band => band.GetBandByName(It.IsAny<string>())).ReturnsAsync(band);

            _albumServiceMock.Setup(album => album.GetAlbumsByDistributorId(It.IsAny<Guid>())).ReturnsAsync(existingAlbums);

            await _service.SynchronizeAllAlbums();

            var expectedAlbumIds = existingAlbums
                .Where(album => album.SKU != "SKU1")
                .Select(album => album.Id);

            _albumServiceMock.Verify(albumService => albumService.UpdateAlbumsStatus(It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(expectedAlbumIds)), AlbumStatus.Unavailable), Times.Once);
        }

        [Fact]
        public async Task SynchronizeAllAlbums_WhenBandDoesNotExist_ShouldCallAddBand()
        {
            var distributor = new Distributor
            {
                Id = Guid.NewGuid(),
                Code = DistributorCode.OsmoseProductions,
                ParsingUrl = "testUrl"
            };

            var distributors = new List<Distributor> { distributor };

            var parsedAlbums = new List<AlbumDto>
            {
                CreateSampleAlbumDto()
            };

            var existingAlbums = new List<Album>();

            var parserMock = new Mock<IParser>();
            parserMock.Setup(parser => parser.ParseAlbums(It.IsAny<string>())).ReturnsAsync(parsedAlbums);

            _parserFactoryMock.Setup(factory => factory.CreateParser(It.IsAny<DistributorCode>())).Returns(parserMock.Object);

            _distributorServiceMock.Setup(service => service.GetAllDistributors()).ReturnsAsync(distributors);

            _bandServiceMock.Setup(service => service.GetBandByName(It.IsAny<string>())).ReturnsAsync((Band) null);

            _albumServiceMock.Setup(service => service.GetAlbumsByDistributorId(It.IsAny<Guid>())).ReturnsAsync(existingAlbums);

            await _service.SynchronizeAllAlbums();

            _bandServiceMock.Verify(bandService => bandService.AddBand(It.Is<Band>(band => band.Name == "Band")), Times.Once);
        }

        private Album CreateSampleAlbum(string sku)
        {
            return new Album
            {
                Id = Guid.NewGuid(),
                SKU = sku,
                BandId = Guid.NewGuid(),
                Price = 17,
                ReleaseDate = DateTime.Now,
                Genre = "Metal",
                PurchaseUrl = "http://testpurchase.com",
                PhotoUrl = "http://testphoto.com",
                Media = MediaType.CD,
                Label = "Test Label",
                Press = "Test Press",
                Description = "Test Description",
                Status = AlbumStatus.Preorder
            };
        }

        private AlbumDto CreateSampleAlbumDto()
        {
            return new AlbumDto
            {
                SKU = "SKU1",
                BandName = "Band",
                Name = "Album1",
                Price = 12,
                ReleaseDate = DateTime.Now,
                Genre = "Metal",
                PurchaseUrl = "http://testpurchase.com",
                PhotoUrl = "http://testphoto.com",
                Media = MediaType.CD,
                Label = "Test Label",
                Press = "Test Press",
                Description = "Test Description",
                Status = AlbumStatus.Restock
            };
        }
    }
}
