using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IBandRepository
    {
        Task<Band> GetById(Guid id);

        Task<IEnumerable<Band>> GetAll();

        Task<Band> GetByName(string bandName);

        Task Add(Band band);

        Task<bool> Update(Band band);

        Task<bool> Delete(Guid id);

        Task<IEnumerable<Band>> GetByFilter(PagingAndSortingFilter filter);
    }
}
