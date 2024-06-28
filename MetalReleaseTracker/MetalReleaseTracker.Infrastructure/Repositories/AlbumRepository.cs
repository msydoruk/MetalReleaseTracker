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
    public class AlbumRepository : IAlbumRepository
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;
        private readonly IMapper _mapper;

        public AlbumRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Album> GetById(Guid id)
        {
            var album = await _dbContext.Albums
                    .Include(a => a.Band)
                    .Include(a => a.Distributor)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);
            return _mapper.Map<Album>(album);
        }

        public async Task<IEnumerable<Album>> GetAll()
        {
            return await _dbContext.Albums
                    .Include(a => a.Band)
                    .Include(a => a.Distributor)
                    .ProjectTo<Album>(_mapper.ConfigurationProvider)
                    .AsNoTracking()
                    .ToListAsync();
        }

        public async Task Add(Album album)
        {
            var albumEntity = _mapper.Map<AlbumEntity>(album);
            await _dbContext.Albums.AddAsync(albumEntity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(Album album)
        {
            var albumEntity = _mapper.Map<AlbumEntity>(album);
            _dbContext.Albums.Update(albumEntity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var album = await _dbContext.Albums.FindAsync(id);
            _dbContext.Albums.Remove(album);
            await _dbContext.SaveChangesAsync();
        }
    }
}
