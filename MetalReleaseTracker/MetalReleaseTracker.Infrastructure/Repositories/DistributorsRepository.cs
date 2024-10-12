using AutoMapper;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class DistributorsRepository : IDistributorsRepository
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;
        private readonly IMapper _mapper;

        public DistributorsRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Distributor> GetById(Guid id)
        {
            var distributor = await _dbContext.Distributors
                   .AsNoTracking()
                   .FirstOrDefaultAsync(distributor => distributor.Id == id);

            return _mapper.Map<Distributor>(distributor);
        }

        public async Task<IEnumerable<Distributor>> GetAll()
        {
            var distributors = await _dbContext.Distributors
                    .AsNoTracking()
                    .ToListAsync();

            return _mapper.Map<IEnumerable<Distributor>>(distributors);
        }

        public async Task Add(Distributor distributor)
        {
            var distributorEntity = _mapper.Map<DistributorEntity>(distributor);
            await _dbContext.Distributors.AddAsync(distributorEntity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> Update(Distributor distributor)
        {
            var existingDistributor = await _dbContext.Distributors.FindAsync(distributor.Id);

            if (existingDistributor == null)
            {
                return false;
            }

            _mapper.Map(distributor, existingDistributor);

            var changes = await _dbContext.SaveChangesAsync();

            return changes > 0;
        }

        public async Task<bool> Delete(Guid id)
        {
            var existingDistributor = await _dbContext.Distributors.FindAsync(id);

            if (existingDistributor == null)
            {
                return false;
            }

            _dbContext.Distributors.Remove(existingDistributor);
            var changes = await _dbContext.SaveChangesAsync();

            return changes > 0;
        }
    }
}
