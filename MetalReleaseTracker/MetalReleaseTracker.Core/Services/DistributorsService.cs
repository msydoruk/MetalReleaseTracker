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

        public async Task<Distributor> GetById(Guid id)
        {
            ValidateGuid(id);

            return await GetExistingDistributorById(id);
        }

        public async Task<IEnumerable<Distributor>> GetAll()
        {
            return await _distributorsRepository.GetAll();
        }

        public async Task Add(Distributor distributor)
        {
            ValidateDistributor(distributor);

            await _distributorsRepository.Add(distributor);
        }

        public async Task<bool> Update(Distributor distributor)
        {
            ValidateDistributor(distributor);

            await GetExistingDistributorById(distributor.Id);

            return await _distributorsRepository.Update(distributor);
        }

        public async Task<bool> Delete(Guid id)
        {
            ValidateGuid(id);

            await GetExistingDistributorById(id);

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

        private async Task<Distributor> GetExistingDistributorById(Guid id)
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
