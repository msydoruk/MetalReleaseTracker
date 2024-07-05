using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<Subscription> GetById(Guid id);

        Task<IEnumerable<Subscription>> GetAll();

        Task Add(Subscription subscription);

        Task<bool> Update(Subscription subscription);

        Task<bool> Delete(Guid id);
    }
}
