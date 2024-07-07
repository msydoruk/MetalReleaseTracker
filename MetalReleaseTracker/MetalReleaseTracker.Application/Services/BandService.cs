using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Services;

namespace MetalReleaseTracker.Application.Services
{
    public class BandService : IBandService
    {
        private readonly IBandRepository _bandRepository;

        public BandService(IBandRepository bandRepository)
        {
            _bandRepository = bandRepository;
        }

        public async Task<Band> GetById(Guid id)
        {
            return await _bandRepository.GetById(id);
        }

        public async Task<IEnumerable<Band>> GetAll()
        {
            return await _bandRepository.GetAll();
        }

        public async Task Add(Band band)
        {
            await _bandRepository.Add(band);
        }

        public async Task<bool> Update(Band band)
        {
            return await _bandRepository.Update(band);
        }

        public async Task<bool> Delete(Guid id)
        {
            return await _bandRepository.Delete(id);
        }
    }
}
