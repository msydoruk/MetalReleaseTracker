using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;

        public AlbumRepository(MetalReleaseTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Album album)
        {
            try
            {
                await _dbContext.Albums.AddAsync(album);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error adding album: {ex.Message}");
                throw new Exception("An error occurred while adding the album.", ex);
            }
        }

        public async Task Delete(Guid id)
        {
            try
            {
                var entity = await _dbContext.Albums.FindAsync(id);
                if (entity == null)
                {
                    throw new KeyNotFoundException($"Album with Id '{id}' not found.");
                }

                _dbContext.Albums.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch (KeyNotFoundException ex)
            {
                Console.Error.WriteLine($"Error deleting album: {ex.Message}");
                throw;
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error deleting album: {ex.Message}");
                throw new Exception("An error occurred while deleting the album.", ex);
            }
        }

        public async Task<IEnumerable<Album>> GetAll()
        {
            try
            {
                return await _dbContext.Albums
                    .Include(a => a.Band)
                    .Include(a => a.Distributor)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving all albums: {ex.Message}");
                throw new Exception("An error occurred while retrieving albums.", ex);
            }
        }

        public async Task<IEnumerable<Album>> GetByBandName(string bandName)
        {
            try
            {
                return await _dbContext.Albums
                    .Include(a => a.Band)
                    .Where(a => a.Band.Name == bandName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving albums by band name '{bandName}': {ex.Message}");
                throw new Exception("An error occurred while retrieving albums by band name.", ex);
            }
        }

        public async Task<IEnumerable<Album>> GetByGenre(string genre)
        {
            try
            {
                return await _dbContext.Albums
                    .Where(a => a.Genre == genre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving albums by genre '{genre}': {ex.Message}");
                throw new Exception("An error occurred while retrieving albums by genre.", ex);
            }
        }

        public async Task<Album> GetById(Guid id)
        {
            try
            {
                var album = await _dbContext.Albums
                    .Include(a => a.Band)
                    .Include(a => a.Distributor)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (album == null)
                {
                    throw new KeyNotFoundException($"Album with Id '{id}' not found.");
                }

                return album;
            }
            catch (KeyNotFoundException ex)
            {
                Console.Error.WriteLine($"Error retrieving album by id '{id}': {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving album by id '{id}': {ex.Message}");
                throw new Exception("An error occurred while retrieving the album by id.", ex);
            }
        }

        public async Task<IEnumerable<Album>> GetByPriceRange(float minPrice, float maxPrice)
        {
            try
            {
                return await _dbContext.Albums
                    .Where(a => a.Price >= minPrice && a.Price <= maxPrice)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving albums by price range '{minPrice} - {maxPrice}': {ex.Message}");
                throw new Exception("An error occurred while retrieving albums by price range.", ex);
            }
        }

        public async Task<IEnumerable<Album>> GetByReleaseDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _dbContext.Albums
                    .Where(a => a.ReleaseDate >= startDate && a.ReleaseDate <= endDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving albums by release date range '{startDate} - {endDate}': {ex.Message}");
                throw new Exception("An error occurred while retrieving albums by release date range.", ex);
            }
        }

        public async Task<IEnumerable<Album>> GetByStatus(AlbumStatus status)
        {
            try
            {
                return await _dbContext.Albums
                    .Where(a => a.Status == status)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving albums by status '{status}': {ex.Message}");
                throw new Exception("An error occurred while retrieving albums by status.", ex);
            }
        }

        public async Task Update(Album album)
        {
            try
            {
                _dbContext.Albums.Update(album);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error updating album: {ex.Message}");
                throw new Exception("An error occurred while updating the album.", ex);
            }
        }
    }
}
