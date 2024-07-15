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

        public async Task<Band> GetByIdBand(Guid id)
        {
            ValidateGuid(id);

            return await EnsureBandExists(id);
        }

        public async Task<IEnumerable<Band>> GetAllBands()
        {
            return await _bandRepository.GetAll();
        }

        public async Task AddBand(Band band)
        {
            ValidateBand(band);

            await _bandRepository.Add(band);
        }

        public async Task<bool> UpdateBand(Band band)
        {
            ValidateBand(band);

            await EnsureBandExists(band.Id);

            return await _bandRepository.Update(band);
        }

        public async Task<bool> DeleteBand(Guid id)
        {
            ValidateGuid(id);

            await EnsureBandExists(id);

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

        private async Task<Band> EnsureBandExists(Guid id)
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
