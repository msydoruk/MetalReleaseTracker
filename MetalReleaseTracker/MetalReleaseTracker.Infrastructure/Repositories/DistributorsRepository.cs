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

        public async Task Add(Distributor distributor)
        {
            var distributorEntity = _mapper.Map<DistributorEntity>(distributor);
            await _dbContext.Distributors.AddAsync(distributorEntity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var distributor = await _dbContext.Distributors.FindAsync(id);
            _dbContext.Distributors.Remove(distributor);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Distributor>> GetAll()
        {
            var distributors = await _dbContext.Distributors
                    .AsNoTracking()
                    .ToListAsync();
            return _mapper.Map<IEnumerable<Distributor>>(distributors);
        }

        public async Task<Distributor> GetById(Guid id)
        {
            var distributor = await _dbContext.Distributors
                   .AsNoTracking()
                   .FirstOrDefaultAsync(d => d.Id == id);
            return _mapper.Map<Distributor>(distributor);
        }

        public async Task Update(Distributor distributor)
        {
            var distributorEntity = _mapper.Map<DistributorEntity>(distributor);
            _dbContext.Distributors.Update(distributorEntity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
