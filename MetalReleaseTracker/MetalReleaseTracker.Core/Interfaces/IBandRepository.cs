using System;
using System.Collections.Generic;
using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IBandRepository
    {
        Task<Band> GetById(Guid id);

        Task<IEnumerable<Band>> GetAll();

        Task Add(Band band);

        Task Update(Band band);

        Task Delete(Guid id);
    }
}
