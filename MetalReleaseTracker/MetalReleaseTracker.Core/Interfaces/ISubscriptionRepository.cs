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
        Task<SubscriptionEntity> GetById(Guid id);

        Task<IEnumerable<SubscriptionEntity>> GetAll();

        Task Add(SubscriptionEntity subscription);

        Task Update(SubscriptionEntity subscription);

        Task Delete(Guid id);
    }
}
