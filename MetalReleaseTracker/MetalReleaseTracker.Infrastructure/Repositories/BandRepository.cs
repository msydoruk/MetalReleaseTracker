using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class BandRepository : IBandRepository
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;

        public BandRepository(MetalReleaseTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Band band)
        {
            try
            {
                await _dbContext.Bands.AddAsync(band);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error adding band: {ex.Message}");
                throw new Exception("An error occurred while adding the band.", ex);
            }
        }

        public async Task Delete(Guid id)
        {
            try
            {
                var entity = await _dbContext.Bands.FindAsync(id);
                if (entity == null)
                {
                    throw new KeyNotFoundException($"Band with Id '{id}' not found.");
                }

                _dbContext.Bands.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch (KeyNotFoundException ex)
            {
                Console.Error.WriteLine($"Error deleting band: {ex.Message}");
                throw;
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error deleting band: {ex.Message}");
                throw new Exception("An error occurred while deleting the band.", ex);
            }
        }

        public async Task<IEnumerable<Band>> GetAll()
        {
            try
            {
                return await _dbContext.Bands.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving all bands: {ex.Message}");
                throw new Exception("An error occurred while retrieving bands.", ex);
            }
        }

        public async Task<Band> GetById(Guid id)
        {
            try
            {
                var band = await _dbContext.Bands.FirstOrDefaultAsync(b => b.Id == id);
                if (band == null)
                {
                    throw new KeyNotFoundException($"Band with Id '{id}' not found.");
                }

                return band;
            }
            catch (KeyNotFoundException ex)
            {
                Console.Error.WriteLine($"Error retrieving band by id '{id}': {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving band by id '{id}': {ex.Message}");
                throw new Exception("An error occurred while retrieving the band by id.", ex);
            }
        }

        public async Task Update(Band band)
        {
            try
            {
                _dbContext.Bands.Update(band);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error updating band: {ex.Message}");
                throw new Exception("An error occurred while updating the band.", ex);
            }
        }
    }
}
