using HtmlAgilityPack;
using MetalReleaseTracker.Infrastructure.Utils;
using MetalReleaseTracker.Infrastructure.Parsers;
using Moq;
using MetalReleaseTracker.Core.Exceptions;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System;

namespace MetalReleaseTracker.Tests.Parsers
{
    public class OsmoseProductionsParserTests
    {
        private readonly OsmoseProductionsParser _parser;
        private readonly Mock<IHtmlLoader> _htmlLoaderMock;
        private readonly Mock<AlbumParser> _albumParserMock;
        private readonly Mock<ILogger<OsmoseProductionsParser>> _loggerMock;

        private const string ParsingUrl = "http://example.com";

        public OsmoseProductionsParserTests()
        {
            _htmlLoaderMock = new Mock<IHtmlLoader>();
            _albumParserMock = new Mock<AlbumParser>();
            _loggerMock = new Mock<ILogger<OsmoseProductionsParser>>();

            _parser = new OsmoseProductionsParser(_htmlLoaderMock.Object, _albumParserMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ParseAlbums_WhenHtmlIsValid_ShouldReturnAlbums()
        {
            var firstPageDocument = CreateHtmlDocument(GetMockAlbumsHtml());
            var albumDetailDocument = CreateHtmlDocument(GetMockAlbumDetailHtml());

            SetupHtmlLoader(ParsingUrl, firstPageDocument);
            SetupHtmlLoader("/album/1", albumDetailDocument);
            SetupHtmlLoader("/album/2", albumDetailDocument);

            var album = await _parser.ParseAlbums(ParsingUrl);

            Assert.NotNull(album);
            Assert.Equal(2, album.Data.Count);
        }

        [Fact]
        public async Task ParseAlbums_WhenHtmlIsEmpty_ShouldThrowOsmoseProductionsParserException()
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
            Assert.Equal(2, albums.Data.Count());
        }

        [Fact]
        public async Task ParseAlbums_WhenHtmlDocumentIsNull_ShouldThrowOsmoseProductionsParserException()
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

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task ParseAlbums_WhenBandNameIsMissing_ShouldReturnEmptyList()
        {
            var albumsHtml = @"
            <html>
                <body>
                    <div class='row GshopListingA'>
                        <div class='column three mobile-four'>
                            <a href='/album/1'></a>
                        </div>
                    </div>
                </body>
            </html>";

            var incompleteAlbumDetailHtml = @"
            <html>
                <body>
                    <div class='column twelve'>
                        <span class='cufonAb'>Album Name</span>
                    </div>
                    <span class='cufonEb'>Press : SR000</span>
                </body>
            </html>";

            var albumsDocument = CreateHtmlDocument(albumsHtml);
            var incompleteAlbumDetailDocument = CreateHtmlDocument(incompleteAlbumDetailHtml);

            SetupHtmlLoader("http://example.com", albumsDocument);
            SetupHtmlLoader("/album/1", incompleteAlbumDetailDocument);

            var albums = await _parser.ParseAlbums("http://example.com");

            Assert.Empty(albums.Data);
        }

        [Fact]
        public async Task ParseAlbums_WhenAlbumNameIsMissing_ShouldReturnEmptyList()
        {
            var albumsHtml = @"
            <html>
                <body>
                    <div class='row GshopListingA'>
                        <div class='column three mobile-four'>
                            <a href='/album/1'></a>
                        </div>
                    </div>
                </body>
            </html>";

            var incompleteAlbumDetailHtml = @"
            <html>
                <body>
                        <span class='cufonAb'>
                            <a href='/band/1'>Test Band</a>
                        </span>
                    <span class='cufonEb'>Press : SR000</span>
                </body>
            </html>";

            var albumsDocument = CreateHtmlDocument(albumsHtml);
            var incompleteAlbumDetailDocument = CreateHtmlDocument(incompleteAlbumDetailHtml);

            SetupHtmlLoader("http://example.com", albumsDocument);
            SetupHtmlLoader("/album/1", incompleteAlbumDetailDocument);

            var albums = await _parser.ParseAlbums("http://example.com");

            Assert.Empty(albums.Data);
        }

        [Fact]
        public async Task ParseAlbums_WhenPriceIsInvalid_ShouldReturnZeroForPrice()
        {
            var albumsHtml = GetMockAlbumsHtml();
            var albumDetailHtml = @"
            <html>
                <body>
                    <span class='cufonAb'>
                        <a>Band Name</a>
                    </span>
                    <div class='column twelve'>
                        <span class='cufonAb'>Album Name</span>
                    </div>
                        <span class='cufonEb'>Press : SR000</span>
                        <span class='cufonCd'>Invalid Price</span>
                </body>
            </html>";

            var albumsDocument = CreateHtmlDocument(albumsHtml);
            var invalidPriceDocument = CreateHtmlDocument(albumDetailHtml);

            SetupHtmlLoader(ParsingUrl, albumsDocument);
            SetupHtmlLoader("/album/1", invalidPriceDocument);
            SetupHtmlLoader("/album/2", invalidPriceDocument);

            var albums = await _parser.ParseAlbums(ParsingUrl);

            Assert.NotNull(albums);
            Assert.Equal(2, albums.Data.Count());
            Assert.All(albums.Data, album => Assert.Equal(0.0f, album.Price));
        }

        [Fact]
        public async Task ParseAlbums_WhenReleaseDateIsInvalid_ShouldReturnDefaultDate()
        {
            var albumsHtml = GetMockAlbumsHtml();
            var albumDetailHtml = @"
            <html>
                <body>
                    <span class='cufonAb'>
                        <a>Band Name</a>
                    </span>
                    <div class='column twelve'>
                        <span class='cufonAb'>Album Name</span>
                    </div>
                    <span class='cufonEb'>Press : SR000</span>
                    <span class='cufonEb'>Year : Invalid Date</span>
                </body>
            </html>";

            var albumsDocument = CreateHtmlDocument(albumsHtml);
            var invalidDateDocument = CreateHtmlDocument(albumDetailHtml);

            SetupHtmlLoader(ParsingUrl, albumsDocument);
            SetupHtmlLoader("/album/1", invalidDateDocument);
            SetupHtmlLoader("/album/2", invalidDateDocument);

            var albums = await _parser.ParseAlbums(ParsingUrl);

            Assert.NotNull(albums);
            Assert.Equal(2, albums.Data.Count());
            Assert.All(albums.Data, album => Assert.Equal(DateTime.MinValue, album.ReleaseDate));
        }

        [Fact]
        public async Task ParseAlbums_WhenGenreAndLabelFieldsAreMissing_ShouldReturnNullForGenreAndLabelFields()
        {
            var albumsHtml = GetMockAlbumsHtml();
            var incompleteAlbumDetailHtml = @"
            <html>
                <body>
                    <span class='cufonAb'>
                        <a>Band Name</a>
                    </span>
                    <div class='column twelve'>
                        <span class='cufonAb'>Album Name</span>
                    </div>
                    <span class='cufonEb'>Press : SR000</span>
                    <span class='cufonEb'>Year : 2022</span>
                </body>
            </html>";

            var albumsDocument = CreateHtmlDocument(albumsHtml);
            var incompleteAlbumDetailDocument = CreateHtmlDocument(incompleteAlbumDetailHtml);

            SetupHtmlLoader(ParsingUrl, albumsDocument);
            SetupHtmlLoader("/album/1", incompleteAlbumDetailDocument);
            SetupHtmlLoader("/album/2", incompleteAlbumDetailDocument);

            var albums = await _parser.ParseAlbums(ParsingUrl);

            Assert.NotNull(albums);
            Assert.Equal(2, albums.Data.Count());

            var album = albums.Data.First();
            Assert.Equal("Album Name", album.Name);
            Assert.Equal("Band Name", album.BandName);
            Assert.Null(album.Genre);
            Assert.Null(album.Label);
        }

        [Fact]
        public async Task ParseAlbums_ShouldHaveDelayBetweenPages()
        {
            var firstPageDocument = CreateHtmlDocument(GetMockPaginationHtml());
            var secondPageDocument = CreateHtmlDocument("<html><body>No more albums</body></html>");
            var albumDetailDocument = CreateHtmlDocument(GetMockAlbumDetailHtml());

            var stopwatch = new Stopwatch();

            _htmlLoaderMock
               .SetupSequence(loader => loader.LoadHtmlDocumentAsync(ParsingUrl))
               .ReturnsAsync(firstPageDocument)
               .ReturnsAsync(secondPageDocument);

            SetupHtmlLoader("/album1", albumDetailDocument);
            SetupHtmlLoader("/album2", albumDetailDocument);
            SetupHtmlLoader("http://example.com?page=2", secondPageDocument);

            stopwatch.Start();
            await _parser.ParseAlbums(ParsingUrl);
            stopwatch.Stop();

            Assert.True(stopwatch.ElapsedMilliseconds >= 1000, "Delay between page requests should be at least 1 second.");
        }

        [Fact]
        public async Task ParseAlbumDetails_WhenPhotoUrlFieldIsMissing_ShouldReturnNull()
        {
            var albumsHtml = GetMockAlbumsHtml();
            var albumDetailHtml = @"
            <html>
                <body>
                    <span class='cufonAb'>
                        <a>Band Name</a>
                    </span>
                    <div class='column twelve'>
                        <span class='cufonAb'>Album Name</span>
                    </div>
                    <span class='cufonEb'>Press : SR000</span>
                </body>
            </html>";

            SetupHtmlLoader(ParsingUrl, CreateHtmlDocument(albumsHtml));
            SetupHtmlLoader("/album/1", CreateHtmlDocument(albumDetailHtml));
            SetupHtmlLoader("/album/2", CreateHtmlDocument(albumDetailHtml));

            var albums = await _parser.ParseAlbums(ParsingUrl);

            Assert.NotNull(albums);
            Assert.Equal(2, albums.Data.Count());
            Assert.Null(albums.Data.First().PhotoUrl);
        }

        [Fact]
        public async Task ParseAlbumDetails_WhenUnsupportedMediaType_ShouldReturnNull()
        {
            var albumsHtml = GetMockAlbumsHtml();
            var albumDetailHtml = @"
            <html>
                <body>
                    <span class='cufonAb'>
                        <a>Band Name</a>
                    </span>
                    <div class='column twelve'>
                        <span class='cufonAb'>Album Name</span>
                    </div>
                    <span class='cufonEb'>Press : SR000</span>
                    <span class='cufonEb'>Media: Unknown Media Type</span>
                </body>
            </html>";

            SetupHtmlLoader(ParsingUrl, CreateHtmlDocument(albumsHtml));
            SetupHtmlLoader("/album/1", CreateHtmlDocument(albumDetailHtml));
            SetupHtmlLoader("/album/2", CreateHtmlDocument(albumDetailHtml));

            var albums = await _parser.ParseAlbums(ParsingUrl);

            Assert.NotNull(albums);
            Assert.Equal(2, albums.Data.Count());
            Assert.Null(albums.Data.First().Media);
        }

        [Fact]
        public async Task ParseAlbumDetails_WhenUnsupportedAlbumStatus_ShouldReturnNull()
        {
            var albumsHtml = GetMockAlbumsHtml();
            var albumDetailHtml = @"
            <html>
                <body>
                    <span class='cufonAb'>
                        <a>Band Name</a>
                    </span>
                    <div class='column twelve'>
                        <span class='cufonAb'>Album Name</span>
                    </div>
                    <span class='cufonEb'>Press : SR000</span>
                    <span class='cufonEb'>New or Used : Unknown</span>
                </body>
            </html>";

            SetupHtmlLoader(ParsingUrl, CreateHtmlDocument(albumsHtml));
            SetupHtmlLoader("/album/1", CreateHtmlDocument(albumDetailHtml));
            SetupHtmlLoader("/album/2", CreateHtmlDocument(albumDetailHtml));

            var albums = await _parser.ParseAlbums(ParsingUrl);

            Assert.NotNull(albums);
            Assert.Equal(2, albums.Data.Count());
            Assert.Null(albums.Data.First().Status);
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
                        <span class='cufonCd'>10 EUR</span>
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