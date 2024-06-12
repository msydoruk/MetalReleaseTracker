using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IBandRepository
    {
        Task<BandEntity> GetByIdAsync(int id);
        Task<IEnumerable<BandEntity>> GetAllAsync();
        Task AddAsync(BandEntity band);
        Task UpdateAsync(BandEntity band);
        Task DeleteAsync(int id);
    }
}
