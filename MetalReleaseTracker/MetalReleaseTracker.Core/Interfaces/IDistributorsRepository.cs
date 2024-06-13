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
        Task<DistributorEntity> GetById(Guid id);

        Task<IEnumerable<DistributorEntity>> GetAll();

        Task Add(DistributorEntity distributor);

        Task Update(DistributorEntity distributor);

        Task Delete(Guid id);
    }
}
