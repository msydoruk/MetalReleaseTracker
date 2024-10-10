using AutoMapper;
using AutoMapper.QueryableExtensions;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
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
                    .Include(album => album.Band)
                    .Include(album => album.Distributor)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(album => album.Id == id);

            return _mapper.Map<Album>(album);
        }

        public async Task<IEnumerable<Album>> GetAll()
        {
            return await _dbContext.Albums
                    .Include(album => album.Band)
                    .Include(album => album.Distributor)
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
                .Include(albumDb => albumDb.Band)
                .Include(albumDb => albumDb.Distributor)
                .FirstOrDefaultAsync(albums => albums.Id == album.Id);

            if (existingAlbum == null)
            {
                return false;
            }

            _mapper.Map(album, existingAlbum);

            var changes = await _dbContext.SaveChangesAsync();

            return changes > 0;
        }

        public async Task<bool> UpdateAlbumsStatus(IEnumerable<Guid> albumsIds, AlbumStatus status)
        {
            var albumIdList = albumsIds.ToList();

            var changes = await _dbContext.Albums
                .Where(album => albumIdList.Contains(album.Id)).ExecuteUpdateAsync(albumDb => albumDb
                    .SetProperty(existingAlbum => existingAlbum.Status, newStatus => status)
                    .SetProperty(existingAlbum => existingAlbum.ModificationTime, time => DateTime.UtcNow));

            return changes > 0;
        }

        public async Task<bool> UpdatePriceForAlbums(IEnumerable<Guid> albumIds, float newPrice)
        {
            var albumIdList = albumIds.ToList();

            var changes = await _dbContext.Albums
                .Where(album => albumIdList.Contains(album.Id))
                .ExecuteUpdateAsync(albumDb => albumDb
                    .SetProperty(existingAlbum => existingAlbum.Price, price => newPrice)
                    .SetProperty(existingAlbum => existingAlbum.ModificationTime, time => DateTime.UtcNow));

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
                .Include(album => album.Band)
                .Include(album => album.Distributor)
                .AsQueryable();

            query = ApplyFilters(query, filter);

            var albums = await query
                .ProjectTo<Album>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return albums;
        }

        public async Task<IEnumerable<Album>> GetByDistributorId(Guid distributorId)
        {
            var albums = await _dbContext.Albums
                .Where(album => album.DistributorId == distributorId)
                .ProjectTo<Album>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return albums;
        }

        private IQueryable<AlbumEntity> ApplyFilters(IQueryable<AlbumEntity> query, AlbumFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.BandName))
            {
                query = query.Where(album => album.Band.Name.IndexOf(filter.BandName, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (filter.ReleaseDateStart.HasValue)
            {
                query = query.Where(album => album.ReleaseDate >= filter.ReleaseDateStart.Value);
            }

            if (filter.ReleaseDateEnd.HasValue)
            {
                query = query.Where(album => album.ReleaseDate <= filter.ReleaseDateEnd.Value);
            }

            if (!string.IsNullOrEmpty(filter.Genre))
            {
                query = query.Where(album => album.Genre.IndexOf(filter.Genre, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (filter.MinimumPrice.HasValue)
            {
                query = query.Where(album => album.Price >= filter.MinimumPrice.Value);
            }

            if (filter.MaximumPrice.HasValue)
            {
                query = query.Where(album => album.Price <= filter.MaximumPrice.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(album => album.Status == filter.Status.Value);
            }

            return query;
        }
    }
}
