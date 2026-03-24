using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class TelegramLinkRepository : ITelegramLinkRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public TelegramLinkRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TelegramLinkEntity?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TelegramLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(link => link.UserId == userId, cancellationToken);
    }

    public async Task<TelegramLinkEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TelegramLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(link => link.ChatId == chatId, cancellationToken);
    }

    public async Task AddAsync(TelegramLinkEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.TelegramLinks.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _dbContext.TelegramLinks
            .Where(link => link.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<Dictionary<string, long>> GetChatIdsByUserIdsAsync(List<string> userIds, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TelegramLinks
            .AsNoTracking()
            .Where(link => userIds.Contains(link.UserId))
            .ToDictionaryAsync(link => link.UserId, link => link.ChatId, cancellationToken);
    }

    public async Task<TelegramLinkTokenEntity?> GetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TelegramLinkTokens
            .FirstOrDefaultAsync(tokenEntity => tokenEntity.Token == token && tokenEntity.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task AddTokenAsync(TelegramLinkTokenEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.TelegramLinkTokens.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveTokenAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        await _dbContext.TelegramLinkTokens
            .Where(token => token.Id == tokenId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task CleanExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.TelegramLinkTokens
            .Where(token => token.ExpiresAt <= DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
