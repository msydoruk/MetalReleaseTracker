using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityServerDbContext _dbContext;

    public RefreshTokenRepository(IdentityServerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefreshTokenEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == token, cancellationToken: cancellationToken);
    }

    public async Task<RefreshTokenEntity?> GetByTokenAndUserIdAsync(string token, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == token && refreshToken.UserId == userId, cancellationToken: cancellationToken);
    }

    public async Task<string> SaveAsync(RefreshTokenEntity refreshTokenEntity, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return refreshTokenEntity.Token;
    }

    public async Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        var storedToken = await GetByTokenAsync(token, cancellationToken);

        if (storedToken == null)
        {
            return false;
        }

        storedToken.IsRevoked = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}