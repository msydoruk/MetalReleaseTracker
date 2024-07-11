using FluentValidation;
using FluentValidation.Results;

using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Validators;

namespace MetalReleaseTracker.Core.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IValidator<Subscription> _subscriptionValidator;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository, IValidator<Subscription> subscriptionValidator)
        {
            _subscriptionRepository = subscriptionRepository;
            _subscriptionValidator = subscriptionValidator;
        }

        public async Task<Subscription> GetById(Guid id)
        {
            var subscription = await _subscriptionRepository.GetById(id);
            if (subscription == null)
            {
                throw new EntityNotFoundException($"Subscription with ID {id} not found.");
            }

            return subscription;
        }

        public async Task<IEnumerable<Subscription>> GetAll()
        {
            return await _subscriptionRepository.GetAll();
        }

        public async Task Add(Subscription subscription)
        {
            ValidateSubscription(subscription);

            await _subscriptionRepository.Add(subscription);
        }

        public async Task<bool> Update(Subscription subscription)
        {
            ValidateSubscription(subscription);

            var existingSubscription = await _subscriptionRepository.GetById(subscription.Id);
            if (existingSubscription == null)
            {
                throw new EntityNotFoundException($"Subscription with ID {subscription.Id} not found.");
            }

            return await _subscriptionRepository.Update(subscription);
        }

        public async Task<bool> Delete(Guid id)
        {
            var subscription = await _subscriptionRepository.GetById(id);
            if (subscription == null)
            {
                throw new EntityNotFoundException($"Subscription with ID {id} not found.");
            }

            return await _subscriptionRepository.Delete(id);
        }

        private void ValidateSubscription(Subscription subscription)
        {
            ValidationResult results = _subscriptionValidator.Validate(subscription);
            if (!results.IsValid)
            {
                throw new ValidationException(results.Errors);
            }
        }
    }
}
