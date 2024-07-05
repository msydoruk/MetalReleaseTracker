using AutoMapper;
using AutoMapper.QueryableExtensions;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

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

        public async Task<bool> Update(Album album)
        {
            var existingAlbum = await _dbContext.Albums
                .Include(a => a.Band)
                .Include(a => a.Distributor)
                .FirstOrDefaultAsync(a => a.Id == album.Id);

            if (existingAlbum == null)
            {
                return false;
            }

            _mapper.Map(album, existingAlbum);

            var changes = await _dbContext.SaveChangesAsync();

            return changes > 0;
        }

        public async Task<bool> Delete(Guid id)
        {
            var existingAlbum = await _dbContext.Albums.FindAsync(id);

            if (existingAlbum == null)
            {
                return false;
            }

            _dbContext.Albums.Remove(existingAlbum);
            var changes = await _dbContext.SaveChangesAsync();

            return changes > 0;
        }

        public async Task<IEnumerable<Album>> GetByFilter(AlbumFilter filter)
        {
            var query = _dbContext.Albums
                .Include(a => a.Band)
                .Include(a => a.Distributor)
                .AsQueryable();

            query = ApplyFilters(query, filter);

            var albums = await query
                .ProjectTo<Album>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return albums;
        }

        private IQueryable<AlbumEntity> ApplyFilters(IQueryable<AlbumEntity> query, AlbumFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.BandName))
            {
                query = query.Where(a => a.Band.Name.IndexOf(filter.BandName, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (filter.ReleaseDateStart.HasValue)
            {
                query = query.Where(a => a.ReleaseDate >= filter.ReleaseDateStart.Value);
            }

            if (filter.ReleaseDateEnd.HasValue)
            {
                query = query.Where(a => a.ReleaseDate <= filter.ReleaseDateEnd.Value);
            }

            if (!string.IsNullOrEmpty(filter.Genre))
            {
                query = query.Where(a => a.Genre.IndexOf(filter.Genre, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (filter.MinimumPrice.HasValue)
            {
                query = query.Where(a => a.Price >= filter.MinimumPrice.Value);
            }

            if (filter.MaximumPrice.HasValue)
            {
                query = query.Where(a => a.Price <= filter.MaximumPrice.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(a => a.Status == filter.Status.Value);
            }

            return query;
        }
    }
}
