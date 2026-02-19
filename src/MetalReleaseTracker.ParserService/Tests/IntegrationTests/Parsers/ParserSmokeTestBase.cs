using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.Results;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Http;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using Microsoft.Extensions.Options;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.IntegrationTests.Parsers;

public abstract class ParserSmokeTestBase
{
    protected static IHtmlDocumentLoader CreateHtmlDocumentLoader()
    {
        var userAgentProvider = new UserAgentProvider();
        var httpSettings = Options.Create(new HttpRequestSettings { RequestTimeoutSeconds = 60 });
        var httpRequestService = new FlurlHttpRequestService(httpSettings, userAgentProvider);
        return new HtmlDocumentLoader(httpRequestService);
    }

    protected static IOptions<GeneralParserSettings> CreateParserSettings()
    {
        return Options.Create(new GeneralParserSettings
        {
            MinDelayBetweenRequestsSeconds = 0,
            MaxDelayBetweenRequestsSeconds = 1
        });
    }

    protected static void AssertListingPageValid(ListingPageResult result)
    {
        Assert.NotNull(result);
        Assert.True(result.Listings.Count > 0, "Expected at least one listing on the page.");

        foreach (var listing in result.Listings)
        {
            Assert.False(string.IsNullOrWhiteSpace(listing.DetailUrl), "Listing DetailUrl should not be empty.");
            Assert.False(string.IsNullOrWhiteSpace(listing.RawTitle), "Listing RawTitle should not be empty.");
            Assert.False(string.IsNullOrWhiteSpace(listing.BandName), "Listing BandName should not be empty.");
            Assert.False(string.IsNullOrWhiteSpace(listing.AlbumTitle), "Listing AlbumTitle should not be empty.");
            Assert.NotNull(listing.MediaType);
        }
    }

    protected static void AssertAlbumDetailValid(AlbumParsedEvent album, DistributorCode expectedCode)
    {
        Assert.NotNull(album);
        Assert.Equal(expectedCode, album.DistributorCode);
        Assert.False(string.IsNullOrWhiteSpace(album.SKU), "SKU should not be empty.");
        Assert.False(string.IsNullOrWhiteSpace(album.BandName), "BandName should not be empty.");
        Assert.False(string.IsNullOrWhiteSpace(album.Name), "Album Name should not be empty.");
        Assert.False(string.IsNullOrWhiteSpace(album.PurchaseUrl), "PurchaseUrl should not be empty.");
        Assert.False(string.IsNullOrWhiteSpace(album.PhotoUrl), "PhotoUrl should not be empty.");
        Assert.True(album.Price > 0, "Price should be greater than zero.");
    }

    protected static void AssertNextPageIsWithinSameCategory(string startUrl, string? nextPageUrl)
    {
        Assert.NotNull(nextPageUrl);

        var startUri = new Uri(startUrl);
        var nextUri = new Uri(nextPageUrl);

        Assert.Equal(startUri.Host, nextUri.Host);

        var startPath = NormalizePathForComparison(startUri.AbsolutePath);
        var nextPath = NormalizePathForComparison(nextUri.AbsolutePath);

        Assert.True(
            nextPath.StartsWith(startPath, StringComparison.OrdinalIgnoreCase),
            $"Next page URL should be within the same category as start URL.\n" +
            $"Start URL: {startUrl}\nNext page URL: {nextPageUrl}");
    }

    protected static void AssertPagesHaveDistinctListings(ListingPageResult firstPage, ListingPageResult secondPage)
    {
        var firstPageUrls = new HashSet<string>(
            firstPage.Listings.Select(listing => listing.DetailUrl),
            StringComparer.OrdinalIgnoreCase);

        var secondPageUrls = new HashSet<string>(
            secondPage.Listings.Select(listing => listing.DetailUrl),
            StringComparer.OrdinalIgnoreCase);

        var overlap = firstPageUrls.Intersect(secondPageUrls, StringComparer.OrdinalIgnoreCase).ToList();

        Assert.True(
            overlap.Count == 0,
            $"Pages should not have overlapping listings. Found {overlap.Count} duplicates: {string.Join(", ", overlap.Take(3))}");
    }

    private static string NormalizePathForComparison(string path)
    {
        return path.TrimEnd('/').Replace(".html", string.Empty);
    }
}
