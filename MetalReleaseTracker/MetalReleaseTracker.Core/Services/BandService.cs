using FluentValidation;
using FluentValidation.Results;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Core.Services
{
    public class BandService : IBandService
    {
        private readonly IBandRepository _bandRepository;
        private readonly IValidator<Band> _bandValidator;

        public BandService(IBandRepository bandRepository, IValidator<Band> bandValidator)
        {
            _bandRepository = bandRepository;
            _bandValidator = bandValidator;
        }

        public async Task<Band> GetById(Guid id)
        {
            ValidateGuid(id);

            return await GetExistingBandById(id);
        }

        public async Task<IEnumerable<Band>> GetAll()
        {
            return await _bandRepository.GetAll();
        }

        public async Task Add(Band band)
        {
            ValidateBand(band);

            await _bandRepository.Add(band);
        }

        public async Task<bool> Update(Band band)
        {
            ValidateBand(band);

            await GetExistingBandById(band.Id);

            return await _bandRepository.Update(band);
        }

        public async Task<bool> Delete(Guid id)
        {
            ValidateGuid(id);

            await GetExistingBandById(id);

            return await _bandRepository.Delete(id);
        }

        private void ValidateBand(Band band)
        {
            ValidationResult results = _bandValidator.Validate(band);
            if (!results.IsValid)
            {
                throw new ValidationException(results.Errors);
            }
        }

        private async Task<Band> GetExistingBandById(Guid id)
        {
            var band = await _bandRepository.GetById(id);
            if (band == null)
            {
                throw new EntityNotFoundException($"Band with ID {id} not found.");
            }

            return band;
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
