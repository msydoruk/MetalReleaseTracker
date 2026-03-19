using MetalReleaseTracker.ParserService.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class BandPhotoEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.BandPhotos.Sync, (
                IServiceScopeFactory serviceScopeFactory,
                IHostApplicationLifetime applicationLifetime) =>
            {
                _ = Task.Run(async () =>
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var syncService = scope.ServiceProvider.GetRequiredService<IBandPhotoSyncService>();
                    await syncService.SyncBandPhotosAsync(applicationLifetime.ApplicationStopping);
                });
                return Results.Accepted();
            })
            .WithName("SyncBandPhotos")
            .WithTags("Admin Band Photos")
            .Produces(202);
    }
}
