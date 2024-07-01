using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IDistributorsRepository
    {
        Task<Distributor> GetById(Guid id);

        Task<IEnumerable<Distributor>> GetAll();

        Task Add(Distributor distributor);

        Task Update(Distributor distributor);

        Task Delete(Guid id);
    }
}
