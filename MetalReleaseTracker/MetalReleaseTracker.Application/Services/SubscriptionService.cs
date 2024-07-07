using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Services;

namespace MetalReleaseTracker.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<Subscription> GetById(Guid id)
        {
            return await _subscriptionRepository.GetById(id);
        }

        public async Task<IEnumerable<Subscription>> GetAll()
        {
            return await _subscriptionRepository.GetAll();
        }

        public async Task Add(Subscription subscription)
        {
            await _subscriptionRepository.Add(subscription);
        }

        public async Task<bool> Update(Subscription subscription)
        {
            return await _subscriptionRepository.Update(subscription);
        }

        public async Task<bool> Delete(Guid id)
        {
            return await _subscriptionRepository.Delete(id);
        }
    }
}
