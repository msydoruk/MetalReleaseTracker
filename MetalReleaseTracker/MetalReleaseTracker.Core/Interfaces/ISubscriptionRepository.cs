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
        Task<SubscriptionEntity> GetByIdAsync(int id);
        Task<IEnumerable<SubscriptionEntity>> GetAllAsync();
        Task AddAsync(SubscriptionEntity album);
        Task UpdateAsync(SubscriptionEntity album);
        Task DeleteAsync(int id);
    }
}
