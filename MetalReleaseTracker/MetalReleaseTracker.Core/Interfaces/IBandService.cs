using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IBandService
    {
        Task<Band> GetByIdBand(Guid id);

        Task<IEnumerable<Band>> GetAllBands();

        Task AddBand(Band band);

        Task<bool> UpdateBand(Band band);

        Task<bool> DeleteBand(Guid id);
    }
}
