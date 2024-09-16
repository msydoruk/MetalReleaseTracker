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

        private const string ParsingUrl = "http://example.com";

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
            var firstPageDocument = CreateHtmlDocument(GetMockAlbumsHtml());
            var albumDetailDocument = CreateHtmlDocument(GetMockAlbumDetailHtml());

            SetupHtmlLoader(ParsingUrl, firstPageDocument);
            SetupHtmlLoader("/album/1", albumDetailDocument);
            SetupHtmlLoader("/album/2", albumDetailDocument);

            var albums = await _parser.ParseAlbums(ParsingUrl);

            Assert.NotNull(albums);
            Assert.Equal(2, albums.Count());
        }

        [Fact]
        public async Task ParseAlbums_WhenHtmlIsEmpty_ShouldReturnEmptyList()
        {
            var emptyHtmlDocument = CreateHtmlDocument("<html><body><div class='row GshopListingA'></div></body></html>");

            SetupHtmlLoader(It.IsAny<string>(), emptyHtmlDocument);

            var albums = await _parser.ParseAlbums(ParsingUrl);

            Assert.Empty(albums);
        }

        [Fact]
        public async Task ParseAlbums_WhenPageHasMultiplePages_ShouldHandlePagination()
        {
            var firstPageDocument = CreateHtmlDocument(GetMockPaginationHtml());
            var secondPageDocument = CreateHtmlDocument("<html><body>No more albums</body></html>");
            var albumDetailDocument = CreateHtmlDocument(GetMockAlbumDetailHtml());

            _htmlLoaderMock
                .SetupSequence(loader => loader.LoadHtmlDocumentAsync(ParsingUrl))
                .ReturnsAsync(firstPageDocument)
                .ReturnsAsync(secondPageDocument);

            SetupHtmlLoader("/album1", albumDetailDocument);
            SetupHtmlLoader("/album2", albumDetailDocument);
            SetupHtmlLoader("http://example.com?page=2", secondPageDocument);

            var albums = await _parser.ParseAlbums(ParsingUrl);

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
                await _parser.ParseAlbums(ParsingUrl));
        }

        [Fact]
        public async Task ParseAlbums_WhenHtmlDocumentIsNull_ShouldThrowException()
        {
            _htmlLoaderMock.Setup(loader => loader.LoadHtmlDocumentAsync(It.IsAny<string>()))
                           .ReturnsAsync((HtmlDocument)null);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _parser.ParseAlbums(ParsingUrl));
            Assert.Equal("Failed to load or parse the HTML document. Album", exception.Message);
        }

        [Fact]
        public async Task ParseAlbums_WhenAlbumNodesAreMissing_ShouldReturnEmptyList()
        {
            var htmlDocument = CreateHtmlDocument("<html><body></body></html>");

            SetupHtmlLoader(It.IsAny<string>(), htmlDocument);

            var result = await _parser.ParseAlbums(ParsingUrl);

            Assert.Empty(result);
        }

        private void SetupHtmlLoader(string url, HtmlDocument document)
        {
            _htmlLoaderMock
                .Setup(loader => loader.LoadHtmlDocumentAsync(url))
                .ReturnsAsync(document);
        }

        private HtmlDocument CreateHtmlDocument(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;
        }

        private string GetMockPaginationHtml()
        {
            return @"
            <html>
                <body>
                    <div class='row GshopListingA'>
                        <div class='column three mobile-four'>
                            <a href='/album1'></a>
                        </div>
                        <div class='column three mobile-four'>
                            <a href='/album2'></a>
                        </div>
                    </div>
                <div class='GtoursPagination'>
                    <a href='http://example.com?page=2'>Next</a>
                </div>
            </body>
        </html>";
        }

        private string GetMockAlbumsHtml()
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