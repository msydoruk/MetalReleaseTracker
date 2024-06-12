using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IAlbumRepository
    {
        Task<AlbumEntity> GetByIdAsync(int id);
        Task<IEnumerable<AlbumEntity>> GetAllAsync();
        Task AddAsync(AlbumEntity album);
        Task UpdateAsync(AlbumEntity album);
        Task DeleteAsync(int id);
    }
}
