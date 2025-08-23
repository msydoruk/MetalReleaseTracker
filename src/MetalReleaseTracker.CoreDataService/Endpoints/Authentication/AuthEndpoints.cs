using MetalReleaseTracker.CoreDataService.Services.Dtos.Authentication;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Authentication;

public static class AuthEndpoints
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapPost(RouteConstants.Api.Auth.LoginWithEmail, async (
                LoginRequestDto request,
                IAuthService authService,
                CancellationToken cancellationToken) =>
            {
                var result = await authService.LoginWithEmailAsync(request, cancellationToken);

                if (result.Success)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);
            })
            .WithName("LoginWithEmail")
            .WithTags("Auth")
            .Produces<AuthResultDto>(200)
            .Produces(400);

        app.MapPost(RouteConstants.Api.Auth.Register, async (
                RegisterRequestDto request,
                IAuthService authService,
                CancellationToken cancellationToken) =>
            {
                var result = await authService.RegisterUserAsync(request, cancellationToken);

                if (result.Success)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);
            })
            .WithName("Register")
            .WithTags("Auth")
            .Produces<AuthResultDto>(200)
            .Produces<AuthResultDto>(400);

        app.MapPost(RouteConstants.Api.Auth.Logout, async (
                IAuthService authService,
                CancellationToken cancellationToken) =>
            {
                await authService.LogoutAsync(cancellationToken: cancellationToken);
                return Results.Ok(new { message = "Logged out successfully" });
            })
            .WithName("Logout")
            .WithTags("Auth")
            .Produces(200);

        app.MapGet(RouteConstants.Api.Auth.GoogleLogin, GoogleAuthHandler.HandleGoogleLogin)
            .WithName("GoogleLogin")
            .WithTags("Auth")
            .Produces(302);

        app.MapGet(RouteConstants.Api.Auth.GoogleAuthComplete, GoogleAuthHandler.HandleGoogleCallback)
            .WithName("GoogleCallback")
            .WithTags("Auth")
            .Produces(302);

        app.MapPost(RouteConstants.Api.Auth.RefreshToken, async (
                RefreshTokenRequestDto request,
                IAuthService authService,
                CancellationToken cancellationToken) =>
            {
                var result = await authService.RefreshTokenAsync(request.RefreshToken, request.UserId, cancellationToken);

                if (result.Success)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);
            })
            .WithName("RefreshToken")
            .WithTags("Auth")
            .Produces<AuthResultDto>(200)
            .Produces<AuthResultDto>(400);

        app.MapPost(RouteConstants.Api.Auth.RevokeToken, async (
                RefreshTokenRequestDto request,
                IAuthService authService,
                CancellationToken cancellationToken) =>
            {
                var result = await authService.RevokeTokenAsync(request.RefreshToken, cancellationToken);

                if (result)
                {
                    return Results.Ok(new { Success = true, Message = "Token revoked successfully" });
                }

                return Results.BadRequest(new { Success = false, Message = "Invalid token" });
            })
            .WithName("RevokeToken")
            .WithTags("Auth")
            .Produces(200)
            .Produces(400);
    }
}