using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticleById;

public static class GetNewsArticleByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.News.GetById, async (
                Guid id,
                GetNewsArticleByIdHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, cancellationToken);
                return result is not null
                    ? Results.Ok(result)
                    : Results.NotFound();
            })
            .WithName("AdminGetNewsArticleById")
            .WithTags("Admin News")
            .Produces<NewsArticleDto>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
