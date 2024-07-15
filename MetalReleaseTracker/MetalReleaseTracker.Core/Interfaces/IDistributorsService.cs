using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IDistributorsService
    {
        Task<Distributor> GetDistributorById(Guid id);

        Task<IEnumerable<Distributor>> GetAllDistributors();

        Task AddDistributor(Distributor distributor);

        Task<bool> UpdateDistributor(Distributor distributor);

        Task<bool> DeleteDistributor(Guid id);
    }
}
