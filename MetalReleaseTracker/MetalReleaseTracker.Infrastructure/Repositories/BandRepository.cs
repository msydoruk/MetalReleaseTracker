using AutoMapper;

using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class BandRepository : IBandRepository
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<BandRepository> _logger;

        public BandRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper, ILogger<BandRepository> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Add(Band band)
        {
            try
            {
                var bandEntity = _mapper.Map<BandEntity>(band);
                await _dbContext.Bands.AddAsync(bandEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while adding a band.");
                throw;
            }
        }

        public async Task Delete(Guid id)
        {
            try
            {
                var band = await _dbContext.Bands.FindAsync(id);
                if (band != null)
                {
                    _dbContext.Bands.Remove(band);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning($"Attempted to delete band with ID {id}, but no matching band was found.");
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a band.");
                throw;
            }
        }

        public async Task<IEnumerable<Band>> GetAll()
        {
            try
            {
                var bands = await _dbContext.Bands
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<IEnumerable<Band>>(bands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all bands.");
                throw;
            }
        }

        public async Task<Band> GetById(Guid id)
        {
            try
            {
                var band = await _dbContext.Bands
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == id);

                return _mapper.Map<Band>(band);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving a band with ID {id}.");
                throw;
            }
        }

        public async Task Update(Band band)
        {
            try
            {
                var bandEntity = _mapper.Map<BandEntity>(band);
                _dbContext.Bands.Update(bandEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while updating a band.");
                throw;
            }
        }
    }
}
