using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.DeleteNewsArticle;

public static class DeleteNewsArticleEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete(AdminRouteConstants.News.Delete, async (
                Guid id,
                DeleteNewsArticleHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, cancellationToken);
                return result
                    ? Results.NoContent()
                    : Results.NotFound();
            })
            .WithName("AdminDeleteNewsArticle")
            .WithTags("Admin News")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
