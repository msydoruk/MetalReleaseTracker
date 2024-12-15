using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IBandService
    {
        Task<Band> GetBandById(Guid id);

        Task<IEnumerable<Band>> GetAllBands();

        Task<Band> GetBandByName(string bandName);

        Task AddBand(Band band);

        Task<bool> UpdateBand(Band band);

        Task<bool> DeleteBand(Guid id);

        Task<IEnumerable<Band>> GetBandsByFilter(PagingAndSortingFilter filter);
    }
}
