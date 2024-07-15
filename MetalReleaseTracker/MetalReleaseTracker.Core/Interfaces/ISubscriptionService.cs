using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface ISubscriptionService
    {
        Task<Subscription> GetByIdSubscription(Guid id);

        Task<IEnumerable<Subscription>> GetAllSubscriptions();

        Task AddSubscription(Subscription subscription);

        Task<bool> UpdateSubscription(Subscription subscription);

        Task<bool> DeleteSubscription(Guid id);
    }
}
