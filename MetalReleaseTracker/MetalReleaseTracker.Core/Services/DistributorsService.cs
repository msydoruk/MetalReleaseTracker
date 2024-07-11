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
            var distributor = await _distributorsRepository.GetById(id);
            if (distributor == null)
            {
                throw new EntityNotFoundException($"Distributor with ID {id} not found.");
            }

            return distributor;
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

            var existingDistributor = await _distributorsRepository.GetById(distributor.Id);
            if (existingDistributor == null)
            {
                throw new EntityNotFoundException($"Distributor with ID {distributor.Id} not found.");
            }

            return await _distributorsRepository.Update(distributor);
        }

        public async Task<bool> Delete(Guid id)
        {
            var distributor = await _distributorsRepository.GetById(id);
            if (distributor == null)
            {
                throw new EntityNotFoundException($"Distributor with ID {id} not found.");
            }

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
    }
}
