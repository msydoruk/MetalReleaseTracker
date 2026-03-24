using MetalReleaseTracker.CoreDataService.Data.Entities;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface ITelegramLinkRepository
{
    Task<TelegramLinkEntity?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<TelegramLinkEntity?> GetByChatIdAsync(long chatId, CancellationToken cancellationToken = default);

    Task AddAsync(TelegramLinkEntity entity, CancellationToken cancellationToken = default);

    Task RemoveByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<Dictionary<string, long>> GetChatIdsByUserIdsAsync(List<string> userIds, CancellationToken cancellationToken = default);

    Task<TelegramLinkTokenEntity?> GetTokenAsync(string token, CancellationToken cancellationToken = default);

    Task AddTokenAsync(TelegramLinkTokenEntity entity, CancellationToken cancellationToken = default);

    Task RemoveTokenAsync(Guid tokenId, CancellationToken cancellationToken = default);

    Task CleanExpiredTokensAsync(CancellationToken cancellationToken = default);
}
