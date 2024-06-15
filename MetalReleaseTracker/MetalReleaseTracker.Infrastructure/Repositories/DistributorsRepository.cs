using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class DistributorsRepository : IDistributorsRepository
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;

        public DistributorsRepository(MetalReleaseTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Distributor distributor)
        {
            try
            {
                await _dbContext.Distributors.AddAsync(distributor);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error adding distributor: {ex.Message}");
                throw new Exception("An error occurred while adding the distributor.", ex);
            }
        }

        public async Task Delete(Guid id)
        {
            try
            {
                var entity = await _dbContext.Distributors.FindAsync(id);
                if (entity == null)
                {
                    throw new KeyNotFoundException($"Distributor with Id '{id}' not found.");
                }

                _dbContext.Distributors.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch (KeyNotFoundException ex)
            {
                Console.Error.WriteLine($"Error deleting distributor: {ex.Message}");
                throw;
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error deleting distributor: {ex.Message}");
                throw new Exception("An error occurred while deleting the distributor.", ex);
            }
        }

        public async Task<IEnumerable<Distributor>> GetAll()
        {
            try
            {
                return await _dbContext.Distributors.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving all distributors: {ex.Message}");
                throw new Exception("An error occurred while retrieving distributors.", ex);
            }
        }

        public async Task<Distributor> GetById(Guid id)
        {
            try
            {
                var distributor = await _dbContext.Distributors.FirstOrDefaultAsync(d => d.Id == id);
                if (distributor == null)
                {
                    throw new KeyNotFoundException($"Distributor with Id '{id}' not found.");
                }

                return distributor;
            }
            catch (KeyNotFoundException ex)
            {
                Console.Error.WriteLine($"Error retrieving distributor by id '{id}': {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving distributor by id '{id}': {ex.Message}");
                throw new Exception("An error occurred while retrieving the distributor by id.", ex);
            }
        }

        public async Task Update(Distributor distributor)
        {
            try
            {
                _dbContext.Distributors.Update(distributor);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error updating distributor: {ex.Message}");
                throw new Exception("An error occurred while updating the distributor.", ex);
            }
        }
    }
}
