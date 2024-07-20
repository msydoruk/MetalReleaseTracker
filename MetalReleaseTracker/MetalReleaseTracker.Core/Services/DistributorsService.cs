using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Core.Services
{
    public class DistributorsService : IDistributorsService
    {
        private readonly IDistributorsRepository _distributorsRepository;
        private readonly IValidationService _validationService;

        public DistributorsService(IDistributorsRepository distributorsRepository, IValidationService validationService)
        {
            _distributorsRepository = distributorsRepository;
            _validationService = validationService;
        }

        public async Task<Distributor> GetDistributorById(Guid id)
        {
            _validationService.Validate(id);

            return await EnsureDistributorExists(id);
        }

        public async Task<IEnumerable<Distributor>> GetAllDistributors()
        {
            return await _distributorsRepository.GetAll();
        }

        public async Task AddDistributor(Distributor distributor)
        {
            _validationService.Validate(distributor);

            await _distributorsRepository.Add(distributor);
        }

        public async Task<bool> UpdateDistributor(Distributor distributor)
        {
            _validationService.Validate(distributor);

            await EnsureDistributorExists(distributor.Id);

            return await _distributorsRepository.Update(distributor);
        }

        public async Task<bool> DeleteDistributor(Guid id)
        {
            _validationService.Validate(id);

            await EnsureDistributorExists(id);

            return await _distributorsRepository.Delete(id);
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
    }
}
