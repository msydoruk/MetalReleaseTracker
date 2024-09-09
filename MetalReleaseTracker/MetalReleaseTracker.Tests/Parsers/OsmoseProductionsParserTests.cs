using HtmlAgilityPack;
using MetalReleaseTracker.Infrastructure.Loaders;
using MetalReleaseTracker.Infrastructure.Parsers;
using MetalReleaseTracker.Infrastructure.Providers;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MetalReleaseTracker.Tests.Parsers
{
    public class OsmoseProductionsParserTests
    {
        private readonly OsmoseProductionsParser _parser;
        private readonly Mock<HtmlLoader> _htmlLoaderMock;
        private readonly Mock<UserAgentProvider> _userAgentProvider;
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly Mock<AlbumParser> _albumParserMock;
        private IConfiguration _configuration;

        public OsmoseProductionsParserTests()
        {
            _httpClientMock = new Mock<HttpClient>();

            var inMemorySettings = new Dictionary<string, string> {
                {"FileSettings:UserAgentsFilePath", "../../../../MetalReleaseTracker.Infrastructure/Resources/user_agents.txt"}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _userAgentProvider = new Mock<UserAgentProvider>(_configuration);

            _htmlLoaderMock = new Mock<HtmlLoader>(_httpClientMock.Object, _userAgentProvider.Object);

            _albumParserMock = new Mock<AlbumParser>();

            _parser = new OsmoseProductionsParser(_htmlLoaderMock.Object, _albumParserMock.Object);
        }

        [Fact]
        public async Task ParseAlbums_WhenHtmlIsValid_ShouldReturnAlbums()
        {
            var firstPageDocument = new HtmlDocument();
            firstPageDocument.LoadHtml(GetMockHtmlWithAlbums());

            var albumDetailDocument = new HtmlDocument();
            albumDetailDocument.LoadHtml(GetMockAlbumDetailHtml());

            _htmlLoaderMock
                .Setup(loader => loader.LoadHtmlDocumentAsync("http://example.com"))
                .ReturnsAsync(firstPageDocument);

            _htmlLoaderMock
                .Setup(loader => loader.LoadHtmlDocumentAsync("/album/1"))
                .ReturnsAsync(albumDetailDocument);

            _htmlLoaderMock
                .Setup(loader => loader.LoadHtmlDocumentAsync("/album/2"))
                .ReturnsAsync(albumDetailDocument);

            var albums = await _parser.ParseAlbums("http://example.com");

            Assert.NotNull(albums);
            Assert.Equal(2, albums.Count());
        }

        [Fact]
        public async Task ParseAlbums_WhenHtmlIsEmpty_ShouldReturnEmptyList()
        {
            var emptyHtmlDocument = new HtmlDocument();
            emptyHtmlDocument.LoadHtml("<html><body><div class='row GshopListingA'></div></body></html>");

            _htmlLoaderMock
                .Setup(loader => loader.LoadHtmlDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(emptyHtmlDocument);

            var albums = await _parser.ParseAlbums("http://example.com");

            Assert.Empty(albums);
        }

        [Fact]
        public async Task ParseAlbums_WhenPageHasMultiplePages_ShouldHandlePagination()
        {
            var firstPageDocument = new HtmlDocument();
            firstPageDocument.LoadHtml(GetMockHtmlWithAlbums());

            var secondPageDocument = new HtmlDocument();
            secondPageDocument.LoadHtml("<html><body>No more albums</body></html>");

            _htmlLoaderMock
                .SetupSequence(loader => loader.LoadHtmlDocumentAsync("http://example.com"))
                .ReturnsAsync(firstPageDocument)
                .ReturnsAsync(secondPageDocument);

            _htmlLoaderMock
                .Setup(loader => loader.LoadHtmlDocumentAsync("http://example.com?page=2"))
                .ReturnsAsync(secondPageDocument);

            var albums = await _parser.ParseAlbums("http://example.com");

            Assert.NotNull(albums);
            Assert.Equal(2, albums.Count());
        }

        [Fact]
        public async Task ParseAlbums_WhenHtmlLoaderFails_ShouldThrowException()
        {
            _htmlLoaderMock
                .Setup(loader => loader.LoadHtmlDocumentAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException());

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _parser.ParseAlbums("http://example.com"));
        }

        private string GetMockHtmlWithAlbums()
        {
            return @"
            <html>
                <body>
                    <div class='row GshopListingA'>
                        <div class='column three mobile-four'>
                            <a href='/album/1'>Album 1</a>
                        </div>
                        <div class='column three mobile-four'>
                            <a href='/album/2'>Album 2</a>
                        </div>
                    </div>
                    <div class='GtoursPagination'>
                        <a href='http://example.com?page=2'>Next Page</a>
                    </div>
                </body>
            </html>";
        }

        private string GetMockAlbumDetailHtml()
        {
            return @"
            <html>
                <body>
                        <span class='cufonAb'>
                            <a>Band Name</a>
                        </span>
                    <div class='column twelve'>
                        <span class='cufonAb'>
                           Album Name
                        </span>
                    </div>
                        <span class='cufonEb'>Press : SR000</span>
                        <span class='cufonEb'>Year : 2022</span>
                        <span class='cufonEb'>Genre : Metal</span>
                        <span class='cufonCd '>10 EUR</span>
                        <a class='lienor' href='http://purchase.com'>Purchase</a>
                    <div class='column left four GshopListingALeft mobile-one'>
                        <img src='http://example.com/photo.jpg' />
                    </div>
                        <span class='cufonEb'>Media: CD</span>
                        <span class='cufonEb'>Label : Label Name</span>
                        <span class='cufonEb'>Press : 12345</span>
                        <span class='cufonEb'>Info : Some info</span>
                        <span class='cufonEb'>New or Used : New</span>
                </body>
            </html>";
        }
    }
}
