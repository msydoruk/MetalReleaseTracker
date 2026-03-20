using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class ChangeLogEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RouteConstants.Api.ChangeLog.GetPaged, async (
                [AsParameters] ChangeLogFilterDto filter,
                IAlbumChangeLogService albumChangeLogService,
                CancellationToken cancellationToken) =>
            {
                var result = await albumChangeLogService.GetChangeLogAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetChangeLog")
            .WithTags("ChangeLog")
            .Produces<PagedResultDto<AlbumChangeLogDto>>();

        endpoints.MapGet(RouteConstants.Api.ChangeLog.GetPriceHistory, async (
                string albumName,
                string bandName,
                IAlbumChangeLogService changeLogService,
                CancellationToken cancellationToken) =>
            {
                var history = await changeLogService.GetPriceHistoryAsync(albumName, bandName, cancellationToken);
                return Results.Ok(history);
            })
            .WithName("GetPriceHistory")
            .WithTags("ChangeLog")
            .Produces<List<PriceHistoryPointDto>>();
    }
}
