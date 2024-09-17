using HtmlAgilityPack;
using MetalReleaseTracker.Infrastructure.Utils;
using MetalReleaseTracker.Infrastructure.Parsers;
using Moq;
using MetalReleaseTracker.Core.Exceptions;

namespace MetalReleaseTracker.Tests.Parsers
{
    public class OsmoseProductionsParserTests
    {
        private readonly OsmoseProductionsParser _parser;
        private readonly Mock<IHtmlLoader> _htmlLoaderMock;
        private readonly Mock<AlbumParser> _albumParserMock;

        private const string ParsingUrl = "http://example.com";

        public OsmoseProductionsParserTests()
        {
            _htmlLoaderMock = new Mock<IHtmlLoader>();

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
        public async Task ParseAlbums_WhenHtmlIsEmpty_ShouldThrowException()
        {
            var emptyHtmlDocument = CreateHtmlDocument("<html><body><div class='row GshopListingA'></div></body></html>");

            SetupHtmlLoader(It.IsAny<string>(), emptyHtmlDocument);

            var exception = await Assert.ThrowsAsync<OsmoseProductionsParserException>(() =>
                _parser.ParseAlbums(ParsingUrl));

            Assert.Equal("Failed to load or parse the HTML document http://example.com", exception.Message);
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
        public async Task ParseAlbums_WhenHtmlDocumentIsNull_ShouldThrowException()
        {
            SetupHtmlLoader(ParsingUrl, null);

            var exception = await Assert.ThrowsAsync<OsmoseProductionsParserException>(() => _parser.ParseAlbums(ParsingUrl));

            Assert.StartsWith("Failed to load or parse the HTML document", exception.Message);
            Assert.Contains(ParsingUrl, exception.Message);
        }

        [Fact]
        public async Task ParseAlbums_WhenAlbumNodesAreMissing_ShouldReturnEmptyList()
        {
            var htmlDocument = CreateHtmlDocument("<html><body><div class='row GshopListingA'></div></body></html>");

            SetupHtmlLoader(ParsingUrl, htmlDocument);

            var result = await _parser.ParseAlbums(ParsingUrl);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ParseAlbums_WhenParseAlbumDetailsReturnsIncompleteData_ShouldReturnAlbumWithNullName()
        {
            var firstPageHtml = @"
            <html>
                <body>
                    <div class='row GshopListingA'>
                        <div class='column three mobile-four'>
                            <a href='/album/1'></a>
                        </div>
                    </div>
                </body>
            </html>";

            var firstPageDocument = CreateHtmlDocument(firstPageHtml);

            var incompleteAlbumDetailHtml = @"
            <html>
                <body>
                    <div class='column twelve'>
                        <span class='cufonAb'></span>
                    </div>
                    <span class='cufonEb'>Press : SR000</span>
                </body>
            </html>";

            var incompleteAlbumDetailDocument = CreateHtmlDocument(incompleteAlbumDetailHtml);

            SetupHtmlLoader(ParsingUrl, firstPageDocument);
            SetupHtmlLoader("/album/1", incompleteAlbumDetailDocument);

            var albums = await _parser.ParseAlbums(ParsingUrl);

            Assert.NotNull(albums);
            Assert.Single(albums);
            Assert.Null(albums.First().Name);
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