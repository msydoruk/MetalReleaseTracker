using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.AiSeo;

public static class AiSeoEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.AiSeo.GenerateBand, async (
                Guid id,
                IAiSeoService aiSeoService,
                CancellationToken cancellationToken) =>
            {
                var result = await aiSeoService.GenerateBandSeoAsync(id, cancellationToken);
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("AdminAiSeoGenerateBand")
            .WithTags("Admin AI SEO")
            .Produces<AiSeoResult>();

        endpoints.MapPost(AdminRouteConstants.AiSeo.GenerateAlbum, async (
                Guid id,
                IAiSeoService aiSeoService,
                CancellationToken cancellationToken) =>
            {
                var result = await aiSeoService.GenerateAlbumSeoAsync(id, cancellationToken);
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("AdminAiSeoGenerateAlbum")
            .WithTags("Admin AI SEO")
            .Produces<AiSeoResult>();

        endpoints.MapPost(AdminRouteConstants.AiSeo.BulkBands, async (
                BulkSeoRequest request,
                IAiSeoService aiSeoService,
                CancellationToken cancellationToken) =>
            {
                var processed = await aiSeoService.GenerateBulkBandSeoAsync(request.Limit, cancellationToken);
                return Results.Ok(new { processed });
            })
            .WithName("AdminAiSeoBulkBands")
            .WithTags("Admin AI SEO");

        endpoints.MapPost(AdminRouteConstants.AiSeo.BulkAlbums, async (
                BulkSeoRequest request,
                IAiSeoService aiSeoService,
                CancellationToken cancellationToken) =>
            {
                var processed = await aiSeoService.GenerateBulkAlbumSeoAsync(request.Limit, cancellationToken);
                return Results.Ok(new { processed });
            })
            .WithName("AdminAiSeoBulkAlbums")
            .WithTags("Admin AI SEO");
    }
}

public class BulkSeoRequest
{
    public int Limit { get; set; } = 50;
}
