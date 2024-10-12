﻿using MetalReleaseTracker.Core.Entities;

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
    }
}
