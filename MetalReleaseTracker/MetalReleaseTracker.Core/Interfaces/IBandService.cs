﻿using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IBandService
    {
        Task<Band> GetById(Guid id);

        Task<IEnumerable<Band>> GetAll();

        Task Add(Band band);

        Task<bool> Update(Band band);

        Task<bool> Delete(Guid id);
    }
}