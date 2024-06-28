using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IAlbumRepository
    {
        Task<Album> GetById(Guid id);

        Task<IEnumerable<Album>> GetAll();

        Task Add(Album album);

        Task Update(Album album);

        Task Delete(Guid id);
    }
}
