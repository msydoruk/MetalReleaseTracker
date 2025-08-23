using MetalReleaseTracker.CoreDataService.Data.Entities;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<RefreshTokenEntity?> GetByTokenAndUserIdAsync(string token, string userId, CancellationToken cancellationToken = default);

    Task<string> SaveAsync(RefreshTokenEntity refreshTokenEntity, CancellationToken cancellationToken = default);

    Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default);
}