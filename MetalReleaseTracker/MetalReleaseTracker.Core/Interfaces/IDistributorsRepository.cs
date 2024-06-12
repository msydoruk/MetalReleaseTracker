using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IDistributorsRepository
    {
        Task<DistributorEntity> GetByIdAsync(int id);
        Task<IEnumerable<DistributorEntity>> GetAllAsync();
        Task AddAsync(DistributorEntity album);
        Task UpdateAsync(DistributorEntity album);
        Task DeleteAsync(int id);
    }
}
