using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.CreateNewsArticle;

public static class CreateNewsArticleEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.News.Create, async (
                CreateNewsArticleRequest request,
                CreateNewsArticleHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return Results.Created($"{AdminRouteConstants.News.GetAll}/{result.Id}", result);
            })
            .WithName("CreateNewsArticle")
            .WithTags("Admin News")
            .Produces<NewsArticleDto>(StatusCodes.Status201Created);
    }
}
