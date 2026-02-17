using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.IntegrationTests.Parsers;

[Trait("Category", "Smoke")]
[Trait("Requires", "Chrome")]
public class BlackMetalVendorParserSmokeTests : ParserSmokeTestBase
{
    private const string StartUrl = "https://black-metal-vendor.com/en/Audio-Records-A-Z/Compact-Disc:::2_122.html";

    private BlackMetalVendorParser CreateParser()
    {
        var userAgentProvider = new Infrastructure.Http.UserAgentProvider();
        var webDriverFactory = new SeleniumWebDriverFactory(userAgentProvider);

        return new BlackMetalVendorParser(
            webDriverFactory,
            NullLogger<BlackMetalVendorParser>.Instance);
    }

    [Fact]
    public async Task ParseListings_FirstPage_ReturnsNonEmptyListings()
    {
        var parser = CreateParser();

        var result = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);

        AssertListingPageValid(result);
    }

    [Fact]
    public async Task ParseListings_Pagination_ReturnsNextPageUrl()
    {
        var parser = CreateParser();

        var firstPage = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);
        Assert.NotNull(firstPage.NextPageUrl);

        var secondPage = await parser.ParseListingsAsync(firstPage.NextPageUrl, CancellationToken.None);
        Assert.True(secondPage.Listings.Count > 0, "Second page should have listings.");
    }

    [Fact]
    public async Task ParseAlbumDetail_SingleProduct_ReturnsPopulatedFields()
    {
        var parser = CreateParser();

        var firstPage = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);
        Assert.True(firstPage.Listings.Count > 0);

        var detailUrl = firstPage.Listings[0].DetailUrl;
        var album = await parser.ParseAlbumDetailAsync(detailUrl, CancellationToken.None);

        AssertAlbumDetailValid(album, DistributorCode.BlackMetalVendor);
    }
}
