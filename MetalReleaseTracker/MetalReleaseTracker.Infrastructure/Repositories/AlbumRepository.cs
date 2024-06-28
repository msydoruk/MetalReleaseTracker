using AutoMapper;
using AutoMapper.QueryableExtensions;

using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class AlbumRepository : BaseRepository<AlbumRepository>, IAlbumRepository
    {
        public AlbumRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper, ILogger<AlbumRepository> logger)
            : base(dbContext, mapper, logger) { }

        public async Task<Album> GetById(Guid id)
        {
            return await HandleDbUpdateException(async () =>
            {
                var album = await DbContext.Albums
                    .Include(a => a.Band)
                    .Include(a => a.Distributor)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (album == null)
                {
                    Logger.LogWarning("Album with Id {Id} not found.", id);
                }

                return Mapper.Map<Album>(album);
            });
        }

        public async Task<IEnumerable<Album>> GetAll()
        {
            return await HandleDbUpdateException(async () =>
            {
                var albums = await DbContext.Albums
                    .Include(a => a.Band)
                    .Include(a => a.Distributor)
                    .ProjectTo<Album>(Mapper.ConfigurationProvider)
                    .AsNoTracking()
                    .ToListAsync();

                return albums;
            });
        }

        public async Task Add(Album album)
        {
            await HandleDbUpdateException(async () =>
            {
                var albumEntity = Mapper.Map<AlbumEntity>(album);
                await DbContext.Albums.AddAsync(albumEntity);
                await DbContext.SaveChangesAsync();
            });
        }

        public async Task Update(Album album)
        {
            await HandleDbUpdateException(async () =>
            {
                var albumEntity = Mapper.Map<AlbumEntity>(album);
                DbContext.Albums.Update(albumEntity);
                await DbContext.SaveChangesAsync();
            });
        }

        public async Task Delete(Guid id)
        {
            await HandleDbUpdateException(async () =>
            {
                var album = await DbContext.Albums.FindAsync(id);
                if (album == null)
                {
                    Logger.LogWarning("Album with Id {Id} not found.", id);
                    throw new KeyNotFoundException($"Album with Id '{id}' not found.");
                }

                DbContext.Albums.Remove(album);
                await DbContext.SaveChangesAsync();
            });
        }
    }
}
