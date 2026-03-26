using System.Security.Claims;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class EmailEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(RouteConstants.Api.Email.Subscribe, async (
                EmailSubscribeRequest request,
                IEmailNotificationService emailNotificationService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return Results.BadRequest("Email is required");
                }

                await emailNotificationService.SubscribeAsync(userId, request.Email, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("SubscribeToEmail")
            .WithTags("Email")
            .Produces(200)
            .Produces(400)
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Email.Verify, async (
                string token,
                IEmailNotificationService emailNotificationService,
                CancellationToken cancellationToken) =>
            {
                var verified = await emailNotificationService.VerifyAsync(token, cancellationToken);
                if (!verified)
                {
                    return Results.BadRequest("Invalid or expired verification token");
                }

                return Results.Ok("Email verified successfully. You can close this page.");
            })
            .WithName("VerifyEmail")
            .WithTags("Email")
            .Produces(200)
            .Produces(400);

        endpoints.MapDelete(RouteConstants.Api.Email.Unsubscribe, async (
                IEmailNotificationService emailNotificationService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await emailNotificationService.UnsubscribeAsync(userId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("UnsubscribeFromEmail")
            .WithTags("Email")
            .Produces(200)
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Email.Status, async (
                IEmailNotificationService emailNotificationService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var status = await emailNotificationService.GetStatusAsync(userId, cancellationToken);
                return Results.Ok(status);
            })
            .RequireAuthorization()
            .WithName("GetEmailStatus")
            .WithTags("Email")
            .Produces(200)
            .Produces(401);
    }
}

public class EmailSubscribeRequest
{
    public string Email { get; set; } = string.Empty;
}
