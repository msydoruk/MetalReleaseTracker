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
        Task<BandEntity> GetById(Guid id);

        Task<IEnumerable<BandEntity>> GetAll();

        Task Add(BandEntity band);

        Task Update(BandEntity band);

        Task Delete(Guid id);
    }
}
