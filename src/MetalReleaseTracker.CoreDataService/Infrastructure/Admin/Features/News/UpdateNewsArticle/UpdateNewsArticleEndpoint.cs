using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.UpdateNewsArticle;

public static class UpdateNewsArticleEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(AdminRouteConstants.News.Update, async (
                Guid id,
                UpdateNewsArticleRequest request,
                UpdateNewsArticleHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, request, cancellationToken);
                return result is not null
                    ? Results.Ok(result)
                    : Results.NotFound();
            })
            .WithName("AdminUpdateNewsArticle")
            .WithTags("Admin News")
            .Produces<NewsArticleDto>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
