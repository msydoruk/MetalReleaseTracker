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
        Task<AlbumEntity> GetById(Guid id);

        Task<IEnumerable<AlbumEntity>> GetAll();

        Task Add(AlbumEntity album);

        Task Update(AlbumEntity album);

        Task Delete(Guid id);
    }
}
