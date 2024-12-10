using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Extensions;

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

        public async Task<IEnumerable<Album>> GetByDistributorId(Guid distributorId)
        {
            var albums = await _dbContext.Albums
                .Where(album => album.DistributorId == distributorId)
                .ProjectTo<Album>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return albums;
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
            var albumIdList = albumsIds.ToHashSet();

            var changes = await _dbContext.Albums
                .Where(album => albumIdList.Contains(album.Id))
                .ExecuteUpdateAsync(albumDb => albumDb
                    .SetProperty(existingAlbum => existingAlbum.Status, newStatus => status)
                    .SetProperty(existingAlbum => existingAlbum.ModificationTime, time => DateTime.UtcNow));

            return changes > 0;
        }

        public async Task<bool> UpdateAlbumPrices(Dictionary<Guid, float> albumPrices)
        {
            var albumIdList = albumPrices.Keys.ToHashSet();

            var changes = await _dbContext.Albums
                .Where(album => albumIdList.Contains(album.Id))
                .ExecuteUpdateAsync(albumDb => albumDb
                    .SetProperty(existingAlbum => existingAlbum.Price, album => albumPrices[album.Id])
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

        public async Task<AlbumFilterResult> GetByFilter(AlbumFilter filter)
        {
            var query = _dbContext.Albums
                .Include(album => album.Band)
                .Include(album => album.Distributor)
                .AsQueryable();

            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(filter.OrderBy) && AllowedSortFields.Contains(filter.OrderBy))
            {
                query = filter.Descending
                    ? query.OrderByDescending(album => EF.Property<object>(album, filter.OrderBy))
                    : query.OrderBy(album => EF.Property<object>(album, filter.OrderBy));
            }

            query = query.Skip(filter.Skip).Take(filter.Take);

            var albums = await query
                .ProjectTo<Album>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return new AlbumFilterResult
            {
                Albums = albums,
                TotalCount = totalCount
            };
        }

        private static readonly HashSet<string> AllowedSortFields =
        [
            nameof(Album.Band),
            nameof(Album.Name),
            nameof(Album.Price),
            nameof(Album.Label),
            nameof(Album.Media)
        ];

        private static IQueryable<AlbumEntity> ApplyFilters(IQueryable<AlbumEntity> query, AlbumFilter filter)
        {
            query = query
                .WhereIf(filter.BandId.HasValue, album => album.BandId == filter.BandId.Value)
                .WhereIf(filter.DistributorId.HasValue, album => album.DistributorId == filter.DistributorId.Value)
                .WhereIf(!string.IsNullOrEmpty(filter.AlbumName), album => album.Name.ToLower().Contains(filter.AlbumName.ToLower()))
                .WhereIf(filter.ReleaseDateStart.HasValue, album => album.ReleaseDate >= filter.ReleaseDateStart.Value)
                .WhereIf(filter.ReleaseDateEnd.HasValue, album => album.ReleaseDate <= filter.ReleaseDateEnd.Value)
                .WhereIf(filter.MinimumPrice.HasValue, album => album.Price >= filter.MinimumPrice.Value)
                .WhereIf(filter.MaximumPrice.HasValue, album => album.Price <= filter.MaximumPrice.Value)
                .WhereIf(filter.Status.HasValue, album => album.Status == filter.Status.Value)
                .WhereIf(filter.Media.HasValue, album => album.Media == filter.Media.Value);

            return query;
        }
    }
}
