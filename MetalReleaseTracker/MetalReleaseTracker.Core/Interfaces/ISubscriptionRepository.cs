using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<Subscription> GetById(Guid id);

        Task<IEnumerable<Subscription>> GetAll();

        Task Add(Subscription subscription);

        Task Update(Subscription subscription);

        Task Delete(Guid id);
    }
}
