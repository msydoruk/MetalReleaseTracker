using AutoMapper;

using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class DistributorsRepository : IDistributorsRepository
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<DistributorsRepository> _logger;

        public DistributorsRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper, ILogger<DistributorsRepository> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Add(Distributor distributor)
        {
            try
            {
                var distributorEntity = _mapper.Map<DistributorEntity>(distributor);
                await _dbContext.Distributors.AddAsync(distributorEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while adding a distributor.");
                throw;
            }
        }

        public async Task Delete(Guid id)
        {
            try
            {
                var distributor = await _dbContext.Distributors.FindAsync(id);
                if (distributor != null)
                {
                    _dbContext.Distributors.Remove(distributor);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning($"Attempted to delete distributor with ID {id}, but no matching distributor was found.");
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a distributor.");
                throw;
            }
        }

        public async Task<IEnumerable<Distributor>> GetAll()
        {
            try
            {
                var distributors = await _dbContext.Distributors
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<IEnumerable<Distributor>>(distributors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all distributors.");
                throw;
            }
        }

        public async Task<Distributor> GetById(Guid id)
        {
            try
            {
                var distributor = await _dbContext.Distributors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == id);

                return _mapper.Map<Distributor>(distributor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving a distributor with ID {id}.");
                throw;
            }
        }

        public async Task Update(Distributor distributor)
        {
            try
            {
                var distributorEntity = _mapper.Map<DistributorEntity>(distributor);
                _dbContext.Distributors.Update(distributorEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while updating a distributor.");
                throw;
            }
        }
    }
}
