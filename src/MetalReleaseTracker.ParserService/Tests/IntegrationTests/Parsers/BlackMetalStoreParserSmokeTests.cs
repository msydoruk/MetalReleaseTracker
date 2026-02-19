using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.IntegrationTests.Parsers;

[Trait("Category", "Smoke")]
public class BlackMetalStoreParserSmokeTests : ParserSmokeTestBase
{
    private const string StartUrl = "https://blackmetalstore.com/categoria-produto/cds/";

    private BlackMetalStoreParser CreateParser()
    {
        return new BlackMetalStoreParser(
            CreateHtmlDocumentLoader(),
            CreateParserSettings(),
            NullLogger<BlackMetalStoreParser>.Instance);
    }

    [Fact]
    public async Task ParseListings_FirstPage_ReturnsNonEmptyListings()
    {
        var parser = CreateParser();

        var result = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);

        AssertListingPageValid(result);
    }

    [Fact]
    public async Task ParseListings_Pagination_ReturnsNextPageWithinSameCategory()
    {
        var parser = CreateParser();

        var firstPage = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);
        AssertNextPageIsWithinSameCategory(StartUrl, firstPage.NextPageUrl);

        var secondPage = await parser.ParseListingsAsync(firstPage.NextPageUrl!, CancellationToken.None);
        AssertListingPageValid(secondPage);
        AssertPagesHaveDistinctListings(firstPage, secondPage);
    }

    [Fact]
    public async Task ParseAlbumDetail_SingleProduct_ReturnsPopulatedFields()
    {
        var parser = CreateParser();

        var firstPage = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);
        Assert.True(firstPage.Listings.Count > 0);

        var detailUrl = firstPage.Listings[0].DetailUrl;
        var album = await parser.ParseAlbumDetailAsync(detailUrl, CancellationToken.None);

        AssertAlbumDetailValid(album, DistributorCode.BlackMetalStore);
    }
}
