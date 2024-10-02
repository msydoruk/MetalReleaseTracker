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
        public async Task ProcessAlbumsFromDistributor_ShouldAddNewAlbums()
        {
            var distributor = new Distributor { Id = Guid.NewGuid(), Code = DistributorCode.OsmoseProductions, ParsingUrl = "testUrl" };

            var parsedAlbums = new List<AlbumDto>
            {
                new AlbumDto 
                { 
                    SKU = "SKU1", 
                    BandName = "Band1", 
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
                    Status = AlbumStatus.Available
                },
            };
            var existingAlbums = new List<Album>();
            var band1 = new Band { Id = Guid.NewGuid(), Name = "Band1" };

            _bandServiceMock.Setup(band => band.GetBandByName("Band1")).ReturnsAsync(band1);
            _parserFactoryMock.Setup(parser => parser.CreateParser(distributor.Code)).Returns(Mock.Of<IParser>());
            _parserFactoryMock.Setup(parser => parser.CreateParser(distributor.Code).ParseAlbums(distributor.ParsingUrl)).ReturnsAsync(parsedAlbums);
            _albumServiceMock.Setup(album => album.GetAllAlbumsFromDistributor(distributor.Id)).ReturnsAsync(existingAlbums);

            var result = await _service.ProcessAlbumsFromDistributor(distributor, distributor.ParsingUrl);

            _albumServiceMock.Verify(album => album.AddAlbum(It.IsAny<Album>()), Times.Exactly(1));
            Assert.Equal(1, result.Count());
        }

        [Fact]
        public async Task ProcessAlbumsFromDistributor_ShouldUpdatesExistingAlbums()
        {
            var distributor = new Distributor { Id = Guid.NewGuid(), Code = DistributorCode.OsmoseProductions, ParsingUrl = "testUrl" };

            var parsedAlbums = new List<AlbumDto>
            {
                new AlbumDto 
                { 
                    SKU = "SKU1", 
                    BandName = "Band1", 
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
                    Status = AlbumStatus.Available
                }
            };
            var band1 = new Band 
            { 
                Id = Guid.NewGuid(), 
                Name = "Band1" 
            };

            _bandServiceMock.Setup(band => band.GetBandByName("Band1")).ReturnsAsync(band1);
            _parserFactoryMock.Setup(parser => parser.CreateParser(distributor.Code)).Returns(Mock.Of<IParser>());
            _parserFactoryMock.Setup(parser => parser.CreateParser(distributor.Code).ParseAlbums(distributor.ParsingUrl)).ReturnsAsync(parsedAlbums);
            _albumServiceMock.Setup(album => album.GetAllAlbumsFromDistributor(distributor.Id)).ReturnsAsync(existingAlbums);

            var result = await _service.ProcessAlbumsFromDistributor(distributor, distributor.ParsingUrl);

            _albumServiceMock.Verify(album => album.UpdateAlbum(It.IsAny<Album>()), Times.Once);
            Assert.Single(result);
        }

        [Fact]
        public async Task ProcessAlbumsFromDistributor_ShouldHidesOldAlbums()
        {
            var distributor = new Distributor { Id = Guid.NewGuid(), Code = DistributorCode.OsmoseProductions, ParsingUrl = "http://example.com" };
            var parsedAlbums = new List<AlbumDto>
            {
                new AlbumDto 
                { 
                    SKU ="SKU1", 
                    BandName = "Band1", 
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
                    Status = AlbumStatus.Restock,
                    IsHidden = false 
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
                    Status = AlbumStatus.Preorder,
                    IsHidden = false 
                }
            };
            var band1 = new Band 
            { 
                Id = Guid.NewGuid(), 
                Name = "Band1" 
            };

            _bandServiceMock.Setup(band => band.GetBandByName("Band1")).ReturnsAsync(band1);
            _parserFactoryMock.Setup(parser => parser.CreateParser(distributor.Code)).Returns(Mock.Of<IParser>());
            _parserFactoryMock.Setup(parser => parser.CreateParser(distributor.Code).ParseAlbums(distributor.ParsingUrl)).ReturnsAsync(parsedAlbums);
            _albumServiceMock.Setup(album => album.GetAllAlbumsFromDistributor(distributor.Id)).ReturnsAsync(existingAlbums);

            await _service.ProcessAlbumsFromDistributor(distributor, distributor.ParsingUrl);

            _albumServiceMock.Verify(album => album.UpdateAlbum(It.Is<Album>(x => x.IsHidden)), Times.Once);
        }
    }
}
