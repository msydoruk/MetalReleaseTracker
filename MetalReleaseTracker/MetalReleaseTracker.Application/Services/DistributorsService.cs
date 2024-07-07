using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Services;

namespace MetalReleaseTracker.Application.Services
{
    public class DistributorsService : IDistributorsService
    {
        private readonly IDistributorsRepository _distributorsRepository;

        public DistributorsService(IDistributorsRepository distributorsRepository)
        {
            _distributorsRepository = distributorsRepository;
        }

        public async Task<Distributor> GetById(Guid id)
        {
            return await _distributorsRepository.GetById(id);
        }

        public async Task<IEnumerable<Distributor>> GetAll()
        {
            return await _distributorsRepository.GetAll();
        }

        public async Task Add(Distributor distributor)
        {
            await _distributorsRepository.Add(distributor);
        }

        public async Task<bool> Update(Distributor distributor)
        {
            return await _distributorsRepository.Update(distributor);
        }

        public async Task<bool> Delete(Guid id)
        {
            return await _distributorsRepository.Delete(id);
        }
    }
}
