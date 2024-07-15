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

        public async Task<Subscription> GetSubscriptionById(Guid id)
        {
            ValidateGuid(id);

            return await EnsureSubscriptionExists(id);
        }

        public async Task<IEnumerable<Subscription>> GetAllSubscriptions()
        {
            return await _subscriptionRepository.GetAll();
        }

        public async Task AddSubscription(Subscription subscription)
        {
            ValidateSubscription(subscription);

            await _subscriptionRepository.Add(subscription);
        }

        public async Task<bool> UpdateSubscription(Subscription subscription)
        {
            ValidateSubscription(subscription);

            await EnsureSubscriptionExists(subscription.Id);

            return await _subscriptionRepository.Update(subscription);
        }

        public async Task<bool> DeleteSubscription(Guid id)
        {
            ValidateGuid(id);

            await EnsureSubscriptionExists(id);

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

        private async Task<Subscription> EnsureSubscriptionExists(Guid id)
        {
            var subscription = await _subscriptionRepository.GetById(id);
            if (subscription == null)
            {
                throw new EntityNotFoundException($"Subscription with ID {id} not found.");
            }

            return subscription;
        }

        private void ValidateGuid(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("The ID must be a non-empty GUID.", nameof(id));
            }
        }
    }
}
