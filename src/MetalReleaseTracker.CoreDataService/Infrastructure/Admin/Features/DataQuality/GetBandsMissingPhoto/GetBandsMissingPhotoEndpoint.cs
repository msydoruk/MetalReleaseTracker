using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetBandsMissingGenre;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetBandsMissingPhoto;

public static class GetBandsMissingPhotoEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.DataQuality.BandsMissingPhoto, async (
                int page,
                int pageSize,
                GetBandsMissingPhotoHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(page, pageSize, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetBandsMissingPhoto")
            .WithTags("Admin Data Quality")
            .Produces<DataQualityPagedResult<DataQualityBandDto>>();
    }
}
