using AutoMapper;

using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class BandRepository : BaseRepository<BandRepository>, IBandRepository
    {
        public BandRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper, ILogger<BandRepository> logger)
            : base(dbContext, mapper, logger) { }

        public async Task Add(Band band)
        {
            await HandleDbUpdateException(async () =>
            {
                var bandEntity = Mapper.Map<BandEntity>(band);
                await DbContext.Bands.AddAsync(bandEntity);
                await DbContext.SaveChangesAsync();
            });
        }

        public async Task Delete(Guid id)
        {
            await HandleDbUpdateException(async () =>
            {
                var band = await DbContext.Bands.FindAsync(id);
                if (band == null)
                {
                    Logger.LogWarning("Band with Id {Id} not found.", id);
                    throw new KeyNotFoundException($"Band with Id '{id}' not found.");
                }

                DbContext.Bands.Remove(band);
                await DbContext.SaveChangesAsync();
            });
        }

        public async Task<IEnumerable<Band>> GetAll()
        {
            return await HandleDbUpdateException(async () =>
            {
                var bands = await DbContext.Bands
                    .AsNoTracking()
                    .ToListAsync();

                return Mapper.Map<IEnumerable<Band>>(bands);
            });
        }

        public async Task<Band> GetById(Guid id)
        {
            return await HandleDbUpdateException(async () =>
            {
                var band = await DbContext.Bands
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (band == null)
                {
                    Logger.LogWarning("Band with Id {Id} not found.", id);
                }

                return Mapper.Map<Band>(band);
            });
        }

        public async Task Update(Band band)
        {
            await HandleDbUpdateException(async () =>
            {
                var bandEntity = Mapper.Map<BandEntity>(band);
                DbContext.Bands.Update(bandEntity);
                await DbContext.SaveChangesAsync();
            });
        }
    }
}
