using MetalReleaseTracker.CoreDataService.Services.Dtos.Authentication;
using Microsoft.AspNetCore.Identity;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IJwtService
{
    JwtTokenDto GenerateJwtToken(IdentityUser user, IList<string> userRoles, string displayName = null);

    string GenerateRefreshToken();

    Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId, CancellationToken cancellationToken = default);

    Task<string> SaveRefreshTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken = default);

    Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}