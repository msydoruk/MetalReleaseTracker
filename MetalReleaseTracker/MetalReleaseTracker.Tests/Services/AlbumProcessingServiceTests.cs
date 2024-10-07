using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Application.Services;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;
using Moq;

namespace MetalReleaseTracker.Tests.Services
{
    public class AlbumProcessingServiceTests
    {
        private readonly Mock<IParserFactory> _parserFactoryMock;
        private readonly Mock<IAlbumService> _albumServiceMock;
        private readonly Mock<IBandService> _bandServiceMock;
        private readonly Mock<IDistributorsService> _distributorServiceMock;
        private readonly AlbumProcessingService _service;

        public AlbumProcessingServiceTests()
        {
            _parserFactoryMock = new Mock<IParserFactory>();
            _albumServiceMock = new Mock<IAlbumService>();
            _bandServiceMock = new Mock<IBandService>();
            _distributorServiceMock = new Mock<IDistributorsService>();

            _service = new AlbumProcessingService(_parserFactoryMock.Object, _albumServiceMock.Object, _bandServiceMock.Object, _distributorServiceMock.Object);
        }

        [Fact]
        public async Task SynchronizeAllAlbums_ShouldAddNewAlbums()
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
                new AlbumDto 
                { 
                    SKU = "SKU1", 
                    BandName = "Band", 
                    Name = "Album1", 
                    Price = 8,
                    ReleaseDate = DateTime.Now,
                    Genre = "Metal",
                    PurchaseUrl = "http://testpurchase.com",
                    PhotoUrl = "http://testphoto.com",
                    Media = MediaType.CD,
                    Label = "Test Label",
                    Press = "Test Press",
                    Description = "Test Description",
                    Status = AlbumStatus.Restock
                },
            };
            var existingAlbums = new List<Album>();
            var band = new Band 
            { 
                Id = Guid.NewGuid(), 
                Name = "Band" 
            };

            var parserMock = new Mock<IParser>();
            parserMock
                .Setup(parser => parser.ParseAlbums(distributor.ParsingUrl))
                .ReturnsAsync(parsedAlbums);

            _parserFactoryMock
                .Setup(factory => factory.CreateParser(distributor.Code))
                .Returns(parserMock.Object);

            _distributorServiceMock
                .Setup(service => service.GetAllDistributors())
                .ReturnsAsync(distributors);

            _bandServiceMock
                .Setup(service => service.GetBandByName(It.IsAny<string>()))
                .ReturnsAsync(band);

            _albumServiceMock
                .Setup(service => service.GetAlbumsByDistributor(distributor.Id))
                .ReturnsAsync(existingAlbums);

            await _service.SynchronizeAllAlbums();

            _albumServiceMock.Verify(album => album.AddAlbum(It.IsAny<Album>()), Times.Once);
        }

        [Fact]
        public async Task SynchronizeAllAlbums_ShouldUpdatesExistingAlbums()
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
                new AlbumDto 
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
                }
            };
            var existingAlbums = new List<Album>
            {
                new Album 
                { 
                    SKU = "SKU1", 
                    BandId = Guid.NewGuid(), 
                    Price = 10,
                    ReleaseDate = DateTime.Now,
                    Genre = "Metal",
                    PurchaseUrl = "http://testpurchase.com",
                    PhotoUrl = "http://testphoto.com",
                    Media = MediaType.CD,
                    Label = "Test Label",
                    Press = "Test Press",
                    Description = "Test Description",
                    Status = AlbumStatus.New
                }
            };

            var band = new Band 
            { 
                Id = Guid.NewGuid(), 
                Name = "Band" 
            };

            var parserMock = new Mock<IParser>();
            parserMock
                .Setup(parser => parser.ParseAlbums(distributor.ParsingUrl))
                .ReturnsAsync(parsedAlbums);

            _parserFactoryMock
                .Setup(factory => factory.CreateParser(distributor.Code))
                .Returns(parserMock.Object);

            _distributorServiceMock
                .Setup(service => service.GetAllDistributors())
                .ReturnsAsync(distributors);

            _bandServiceMock.Setup(band => band.GetBandByName(It.IsAny<string>()))
                .ReturnsAsync(band);

            _albumServiceMock.Setup(album => album.GetAlbumsByDistributor(distributor.Id))
                .ReturnsAsync(existingAlbums);

            await _service.SynchronizeAllAlbums();

            _albumServiceMock.Verify(album => album.UpdateAlbum(It.IsAny<Album>()), Times.Once);
        }

        [Fact]
        public async Task SynchronizeAllAlbums_ShouldHidesOldAlbums()
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
                new AlbumDto 
                { 
                    SKU ="SKU1", 
                    BandName = "Band", 
                    Name = "Album1", 
                    Price = 10,
                    ReleaseDate = DateTime.Now,
                    Genre = "Metal",
                    PurchaseUrl = "http://testpurchase.com",
                    PhotoUrl = "http://testphoto.com",
                    Media = MediaType.LP,
                    Label = "Test Label",
                    Press = "Test Press",
                    Description = "Test Description",
                    Status = AlbumStatus.New
                }
            };
            var existingAlbums = new List<Album>
            {
                new Album 
                { 
                    SKU = "SKU1", 
                    BandId = Guid.NewGuid(),
                    Price = 11,
                    ReleaseDate = DateTime.Now,
                    Genre = "Metal",
                    PurchaseUrl = "http://testpurchase.com",
                    PhotoUrl = "http://testphoto.com",
                    Media = MediaType.Tape,
                    Label = "Test Label",
                    Press = "Test Press",
                    Description = "Test Description",
                    Status = AlbumStatus.Restock
                },
                new Album 
                { 
                    SKU = "SKU2", 
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
                }
            };
            var band = new Band 
            { 
                Id = Guid.NewGuid(), 
                Name = "Band" 
            };

            var parserMock = new Mock<IParser>();
            parserMock
                .Setup(parser => parser.ParseAlbums(distributor.ParsingUrl))
                .ReturnsAsync(parsedAlbums);

            _parserFactoryMock
                .Setup(factory => factory.CreateParser(distributor.Code))
                .Returns(parserMock.Object);

            _distributorServiceMock
                .Setup(service => service.GetAllDistributors())
                .ReturnsAsync(distributors);

            _bandServiceMock.Setup(band => band.GetBandByName(It.IsAny<string>()))
                .ReturnsAsync(band);

            _albumServiceMock.Setup(album => album.GetAlbumsByDistributor(distributor.Id))
                .ReturnsAsync(existingAlbums);

            await _service.SynchronizeAllAlbums();

            _albumServiceMock.Verify(album => album.UpdateAlbums(It.IsAny<IEnumerable<Album>>()), Times.Once);
        }

        [Fact]
        public async Task SynchronizeAllAlbums_ShouldCallAddBand_WhenBandDoesNotExist()
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
                new AlbumDto
                {
                    SKU = "SKU1",
                    BandName = "Band",
                    Name = "Album1",
                    Price = 10,
                    ReleaseDate = DateTime.Now,
                    Genre = "Metal",
                    PurchaseUrl = "http://testpurchase.com",
                    PhotoUrl = "http://testphoto.com",
                    Media = MediaType.CD,
                    Label = "Test Label",
                    Press = "Test Press",
                    Description = "Test Description",
                    Status = AlbumStatus.New
                }
            };

            var existingAlbums = new List<Album>();

            Band band = null;

            var parserMock = new Mock<IParser>();
            parserMock
                .Setup(parser => parser.ParseAlbums(distributor.ParsingUrl))
                .ReturnsAsync(parsedAlbums);

            _parserFactoryMock
                .Setup(factory => factory.CreateParser(distributor.Code))
                .Returns(parserMock.Object);

            _distributorServiceMock
                 .Setup(service => service.GetAllDistributors())
                 .ReturnsAsync(distributors);

            _bandServiceMock
                .Setup(service => service.GetBandByName(It.IsAny<string>()))
                .ReturnsAsync(band);

            _albumServiceMock
                .Setup(service => service.GetAlbumsByDistributor(distributor.Id))
                .ReturnsAsync(existingAlbums);

            await _service.SynchronizeAllAlbums();

            _bandServiceMock.Verify(bandService => bandService.AddBand(It.Is<Band>(band => band.Name == "Band")), Times.Once);
        }
    }
}
