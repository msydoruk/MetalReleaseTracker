using AutoMapper;

using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class DistributorsRepository : BaseRepository<DistributorsRepository>, IDistributorsRepository
    {
        public DistributorsRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper, ILogger<DistributorsRepository> logger)
            : base(dbContext, mapper, logger) { }

        public async Task Add(Distributor distributor)
        {
            await HandleDbUpdateException(async () =>
            {
                var distributorEntity = Mapper.Map<DistributorEntity>(distributor);
                await DbContext.Distributors.AddAsync(distributorEntity);
                await DbContext.SaveChangesAsync();
            });
        }

        public async Task Delete(Guid id)
        {
            await HandleDbUpdateException(async () =>
            {
                var distributor = await DbContext.Distributors.FindAsync(id);
                if (distributor == null)
                {
                    Logger.LogWarning("Distributor with Id {Id} not found.", id);
                    throw new KeyNotFoundException($"Distributor with Id '{id}' not found.");
                }

                DbContext.Distributors.Remove(distributor);
                await DbContext.SaveChangesAsync();
            });
        }

        public async Task<IEnumerable<Distributor>> GetAll()
        {
            return await HandleDbUpdateException(async () =>
            {
                var distributors = await DbContext.Distributors
                    .AsNoTracking()
                    .ToListAsync();

                return Mapper.Map<IEnumerable<Distributor>>(distributors);
            });
        }

        public async Task<Distributor> GetById(Guid id)
        {
            return await HandleDbUpdateException(async () =>
            {
                var distributor = await DbContext.Distributors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (distributor == null)
                {
                    Logger.LogWarning("Distributor with Id {Id} not found.", id);
                }

                return Mapper.Map<Distributor>(distributor);
            });
        }

        public async Task Update(Distributor distributor)
        {
            await HandleDbUpdateException(async () =>
            {
                var distributorEntity = Mapper.Map<DistributorEntity>(distributor);
                DbContext.Distributors.Update(distributorEntity);
                await DbContext.SaveChangesAsync();
            });
        }
    }
}
