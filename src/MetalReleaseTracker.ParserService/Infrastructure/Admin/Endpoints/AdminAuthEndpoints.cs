using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class AdminAuthEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.Auth.Login, (
                LoginRequestDto request,
                IAdminAuthService authService) =>
            {
                var result = authService.Login(request.Username, request.Password);

                return result is null
                    ? Results.Unauthorized()
                    : Results.Ok(result);
            })
            .AllowAnonymous()
            .WithName("AdminLogin")
            .WithTags("Admin Auth")
            .Produces<LoginResponseDto>()
            .Produces(401);
    }
}
