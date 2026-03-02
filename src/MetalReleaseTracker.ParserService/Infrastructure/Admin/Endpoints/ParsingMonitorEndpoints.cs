using System.Text.Json;
using System.Text.Json.Serialization;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class ParsingMonitorEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.ParsingMonitor.Live, async (
                IParsingProgressTracker tracker,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                httpContext.Response.ContentType = "text/event-stream";
                httpContext.Response.Headers.CacheControl = "no-cache";
                httpContext.Response.Headers.Connection = "keep-alive";

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                };

                await foreach (var progressEvent in tracker.SubscribeAsync(cancellationToken))
                {
                    var eventName = progressEvent.Type.ToString().ToLowerInvariant();
                    var json = JsonSerializer.Serialize(progressEvent, jsonOptions);
                    await httpContext.Response.WriteAsync($"event: {eventName}\ndata: {json}\n\n", cancellationToken);
                    await httpContext.Response.Body.FlushAsync(cancellationToken);
                }
            })
            .WithName("ParsingMonitorLive")
            .WithTags("Admin Parsing Monitor");

        endpoints.MapGet(AdminRouteConstants.ParsingMonitor.Runs, async (
                int page,
                int pageSize,
                IAdminQueryRepository repository,
                CancellationToken cancellationToken) =>
            {
                var result = await repository.GetParsingRunsAsync(page, pageSize, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetParsingRuns")
            .WithTags("Admin Parsing Monitor")
            .Produces<PagedResultDto<ParsingRunDto>>();
    }
}
