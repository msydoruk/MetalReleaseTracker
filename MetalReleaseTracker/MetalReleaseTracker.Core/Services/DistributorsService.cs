using FluentValidation;
using FluentValidation.Results;

using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Validators;

namespace MetalReleaseTracker.Core.Services
{
    public class DistributorsService : IDistributorsService
    {
        private readonly IDistributorsRepository _distributorsRepository;
        private readonly IValidator<Distributor> _distributorValidator;

        public DistributorsService(IDistributorsRepository distributorsRepository, IValidator<Distributor> distributorValidator)
        {
            _distributorsRepository = distributorsRepository;
            _distributorValidator = distributorValidator;
        }

        public async Task<Distributor> GetByIdDistributor(Guid id)
        {
            ValidateGuid(id);

            return await EnsureDistributorExists(id);
        }

        public async Task<IEnumerable<Distributor>> GetAllDistributors()
        {
            return await _distributorsRepository.GetAll();
        }

        public async Task AddDistributor(Distributor distributor)
        {
            ValidateDistributor(distributor);

            await _distributorsRepository.Add(distributor);
        }

        public async Task<bool> UpdateDistributor(Distributor distributor)
        {
            ValidateDistributor(distributor);

            await EnsureDistributorExists(distributor.Id);

            return await _distributorsRepository.Update(distributor);
        }

        public async Task<bool> DeleteDistributor(Guid id)
        {
            ValidateGuid(id);

            await EnsureDistributorExists(id);

            return await _distributorsRepository.Delete(id);
        }

        private void ValidateDistributor(Distributor distributor)
        {
            ValidationResult results = _distributorValidator.Validate(distributor);
            if (!results.IsValid)
            {
                throw new ValidationException(results.Errors);
            }
        }

        private async Task<Distributor> EnsureDistributorExists(Guid id)
        {
            var distributor = await _distributorsRepository.GetById(id);
            if (distributor == null)
            {
                throw new EntityNotFoundException($"Distributor with ID {id} not found.");
            }

            return distributor;
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
