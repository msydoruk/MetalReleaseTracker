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
        private readonly ILogger<AlbumRepository> _logger;

        public AlbumRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper, ILogger<AlbumRepository> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Album> GetById(Guid id)
        {
            try
            {
                var album = await _dbContext.Albums
                    .Include(a => a.Band)
                    .Include(a => a.Distributor)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);
                return _mapper.Map<Album>(album);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving an album with ID {id}.");
                throw;
            }
        }

        public async Task<IEnumerable<Album>> GetAll()
        {
            try
            {
                return await _dbContext.Albums
                    .Include(a => a.Band)
                    .Include(a => a.Distributor)
                    .ProjectTo<Album>(_mapper.ConfigurationProvider)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all albums.");
                throw;
            }
        }

        public async Task<IEnumerable<Album>> GetByReleaseDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                var albums = await _dbContext.Albums
                    .Where(a => a.ReleaseDate >= startDate && a.ReleaseDate <= endDate)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<IEnumerable<Album>>(albums);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving albums within the date range {startDate} - {endDate}.");
                throw;
            }
        }

        public async Task<IEnumerable<Album>> GetByStatus(AlbumStatus status)
        {
            try
            {
                var albums = await _dbContext.Albums
                    .Where(a => a.Status == status)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<IEnumerable<Album>>(albums);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving albums by status {status}.");
                throw;
            }
        }

        public async Task Add(Album album)
        {
            try
            {
                var albumEntity = _mapper.Map<AlbumEntity>(album);
                await _dbContext.Albums.AddAsync(albumEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while adding an album.");
                throw;
            }
        }

        public async Task Update(Album album)
        {
            try
            {
                var albumEntity = _mapper.Map<AlbumEntity>(album);
                _dbContext.Albums.Update(albumEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while updating an album.");
                throw;
            }
        }

        public async Task Delete(Guid id)
        {
            try
            {
                var album = await _dbContext.Albums.FindAsync(id);
                if (album != null)
                {
                    _dbContext.Albums.Remove(album);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning($"Attempted to delete album with ID {id}, but no matching album was found.");
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while deleting an album.");
                throw;
            }
        }
    }
}
