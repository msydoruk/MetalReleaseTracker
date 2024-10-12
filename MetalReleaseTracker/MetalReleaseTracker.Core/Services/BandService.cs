using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Core.Services
{
    public class BandService : IBandService
    {
        private readonly IBandRepository _bandRepository;
        private readonly IValidationService _validationService;

        public BandService(IBandRepository bandRepository, IValidationService validationService)
        {
            _bandRepository = bandRepository;
            _validationService = validationService;
        }

        public async Task<Band> GetBandById(Guid id)
        {
            _validationService.Validate(id);

            return await EnsureBandExists(id);
        }

        public async Task<IEnumerable<Band>> GetAllBands()
        {
            return await _bandRepository.GetAll();
        }

        public async Task<Band> GetBandByName(string bandName)
        {
            if (string.IsNullOrEmpty(bandName))
            {
                throw new ArgumentException("Band name cannot be empty or null.", nameof(bandName));
            }

            return await EnsureBandExistsByName(bandName);
        }

        public async Task AddBand(Band band)
        {
            _validationService.Validate(band);

            await _bandRepository.Add(band);
        }

        public async Task<bool> UpdateBand(Band band)
        {
            _validationService.Validate(band);

            await EnsureBandExists(band.Id);

            return await _bandRepository.Update(band);
        }

        public async Task<bool> DeleteBand(Guid id)
        {
            _validationService.Validate(id);

            await EnsureBandExists(id);

            return await _bandRepository.Delete(id);
        }

        private async Task<Band> EnsureBandExists(Guid id)
        {
            var band = await _bandRepository.GetById(id);
            if (band == null)
            {
                throw new EntityNotFoundException($"Band with ID {id} not found.");
            }

            return band;
        }

        private async Task<Band> EnsureBandExistsByName(string bandName)
        {
            var band = await _bandRepository.GetByName(bandName);
            if (band == null)
            {
                throw new EntityNotFoundException($"Band with name '{bandName}' not found.");
            }

            return band;
        }
    }
}
