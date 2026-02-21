using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class BandReferenceEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.BandReferences.GetAll, async (
                [AsParameters] BandReferenceFilterDto filter,
                IAdminQueryRepository queryRepository,
                CancellationToken cancellationToken) =>
            {
                var result = await queryRepository.GetBandReferencesAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetBandReferences")
            .WithTags("Admin Band References")
            .Produces<PagedResultDto<BandReferenceDto>>();

        endpoints.MapGet(AdminRouteConstants.BandReferences.GetById, async (
                Guid id,
                IAdminQueryRepository queryRepository,
                CancellationToken cancellationToken) =>
            {
                var result = await queryRepository.GetBandReferenceByIdAsync(id, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("GetBandReferenceById")
            .WithTags("Admin Band References")
            .Produces<BandReferenceDetailDto>()
            .Produces(404);
    }
}
