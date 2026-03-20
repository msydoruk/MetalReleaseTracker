using System.Text;
using System.Xml.Linq;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints;

public static class FeedEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RouteConstants.Api.Feed.Rss, async (
                IAlbumChangeLogService albumChangeLogService,
                CancellationToken cancellationToken) =>
            {
                var filter = new ChangeLogFilterDto(Page: 1, PageSize: 50);
                var changelog = await albumChangeLogService.GetChangeLogAsync(filter, cancellationToken);

                var items = changelog.Items.Select(item =>
                    new XElement("item",
                        new XElement("title", $"{item.BandName} - {item.AlbumName} ({item.ChangeType})"),
                        new XElement("description", $"{item.ChangeType}: {item.BandName} - {item.AlbumName} at {item.DistributorName}"),
                        new XElement("link", item.PurchaseUrl ?? "https://metal-release.com/albums"),
                        new XElement("pubDate", item.ChangedAt.ToString("R")),
                        new XElement("guid", new XAttribute("isPermaLink", "false"), item.Id.ToString())));

                var rss = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(
                        "rss",
                        new XAttribute("version", "2.0"),
                        new XElement("channel",
                            new XElement("title", "Metal Release Tracker"),
                            new XElement("link", "https://metal-release.com"),
                            new XElement("description", "Ukrainian metal releases from foreign distributors"),
                            new XElement("language", "en"),
                            items)));

                var xml = rss.Declaration + "\n" + rss.ToString();
                return Results.Content(xml, "application/rss+xml", Encoding.UTF8);
            })
            .WithName("GetRssFeed")
            .WithTags("Feed")
            .Produces<string>(200, "application/rss+xml");
    }
}
