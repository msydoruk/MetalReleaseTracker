using System.Text.Json;
using System.Threading.Channels;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class AiVerificationEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.AiVerification.GetAll, async (
                [AsParameters] AiVerificationFilterDto filter,
                IAiVerificationService aiVerificationService,
                CancellationToken cancellationToken) =>
            {
                var result = await aiVerificationService.GetVerificationsAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetAiVerifications")
            .WithTags("Admin AI Verification")
            .Produces<PagedResultDto<AiVerificationDto>>();

        endpoints.MapPost(AdminRouteConstants.AiVerification.Run, async (
                RunVerificationDto request,
                IAiVerificationService aiVerificationService,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                httpContext.Response.ContentType = "text/event-stream";
                httpContext.Response.Headers.CacheControl = "no-cache";
                httpContext.Response.Headers.Connection = "keep-alive";

                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var channel = Channel.CreateUnbounded<VerificationProgressEvent>();

                var processingTask = aiVerificationService.RunVerificationStreamAsync(
                    request, channel.Writer, cancellationToken);

                await foreach (var progressEvent in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    var json = JsonSerializer.Serialize(progressEvent, jsonOptions);
                    await httpContext.Response.WriteAsync($"event: {progressEvent.Type}\ndata: {json}\n\n", cancellationToken);
                    await httpContext.Response.Body.FlushAsync(cancellationToken);
                }

                await processingTask;
            })
            .WithName("RunAiVerification")
            .WithTags("Admin AI Verification");

        endpoints.MapPut(AdminRouteConstants.AiVerification.SetDecision, async (
                Guid id,
                SetDecisionDto request,
                IAiVerificationService aiVerificationService,
                CancellationToken cancellationToken) =>
            {
                await aiVerificationService.SetDecisionAsync(id, request.Decision, cancellationToken);
                return Results.NoContent();
            })
            .WithName("SetAiVerificationDecision")
            .WithTags("Admin AI Verification")
            .Produces(204);

        endpoints.MapPut(AdminRouteConstants.AiVerification.BatchDecision, async (
                BatchSetDecisionDto request,
                IAiVerificationService aiVerificationService,
                CancellationToken cancellationToken) =>
            {
                await aiVerificationService.SetBatchDecisionAsync(request.Ids, request.Decision, cancellationToken);
                return Results.NoContent();
            })
            .WithName("BatchSetAiVerificationDecision")
            .WithTags("Admin AI Verification")
            .Produces(204);

        endpoints.MapPut(AdminRouteConstants.AiVerification.BulkDecision, async (
                BulkSetDecisionDto request,
                IAiVerificationService aiVerificationService,
                CancellationToken cancellationToken) =>
            {
                var count = await aiVerificationService.SetBulkDecisionByFilterAsync(
                    request.DistributorCode,
                    request.IsUkrainian,
                    request.Decision,
                    cancellationToken);
                return Results.Ok(new { count });
            })
            .WithName("BulkSetAiVerificationDecision")
            .WithTags("Admin AI Verification")
            .Produces<object>();
    }
}
