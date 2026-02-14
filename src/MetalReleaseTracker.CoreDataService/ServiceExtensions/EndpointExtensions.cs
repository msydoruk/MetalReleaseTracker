using MetalReleaseTracker.CoreDataService.Endpoints.Authentication;
using MetalReleaseTracker.CoreDataService.Endpoints.Catalog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions;

public static class EndpointExtensions
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        MapAuthenticationEndpoints(app);
        var catalogGroup = app.MapGroup(string.Empty);
        MapCatalogEndpoints(catalogGroup);

        return app;
    }

    private static void MapAuthenticationEndpoints(WebApplication app)
    {
        AuthEndpoints.MapEndpoints(app);
    }

    private static void MapCatalogEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        AlbumEndpoints.MapEndpoints(routeBuilder);
        BandEndpoints.MapEndpoints(routeBuilder);
        DistributorEndpoints.MapEndpoints(routeBuilder);
    }
}