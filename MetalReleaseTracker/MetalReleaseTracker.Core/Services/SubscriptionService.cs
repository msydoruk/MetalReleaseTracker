using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Core.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IValidationService _validationService;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository, IValidationService validationService)
        {
            _subscriptionRepository = subscriptionRepository;
            _validationService = validationService;
        }

        public async Task<Subscription> GetSubscriptionById(Guid id)
        {
            _validationService.Validate(id);

            return await EnsureSubscriptionExists(id);
        }

        public async Task<IEnumerable<Subscription>> GetAllSubscriptions()
        {
            return await _subscriptionRepository.GetAll();
        }

        public async Task AddSubscription(Subscription subscription)
        {
            _validationService.Validate(subscription);

            await _subscriptionRepository.Add(subscription);
        }

        public async Task<bool> UpdateSubscription(Subscription subscription)
        {
            _validationService.Validate(subscription);

            await EnsureSubscriptionExists(subscription.Id);

            return await _subscriptionRepository.Update(subscription);
        }

        public async Task<bool> DeleteSubscription(Guid id)
        {
            _validationService.Validate(id);

            await EnsureSubscriptionExists(id);

            return await _subscriptionRepository.Delete(id);
        }

        private async Task<Subscription> EnsureSubscriptionExists(Guid id)
        {
            var subscription = await _subscriptionRepository.GetById(id);
            if (subscription == null)
            {
                throw new EntityNotFoundException($"Subscription with ID {id} not found.");
            }

            return subscription;
        }
    }
}
