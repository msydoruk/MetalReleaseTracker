using MetalReleaseTracker.CoreDataService.Services.Dtos.Authentication;
using Microsoft.AspNetCore.Http;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginWithEmailAsync(LoginRequestDto requestDto, CancellationToken cancellationToken = default);

    Task<AuthResultDto> RegisterUserAsync(RegisterRequestDto requestDto, CancellationToken cancellationToken = default);

    Task<AuthResultDto> RefreshTokenAsync(string refreshToken, string userId, CancellationToken cancellationToken = default);

    Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task<AuthResultDto> LoginWithGoogleAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}