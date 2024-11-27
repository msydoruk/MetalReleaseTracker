using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MetalReleaseTracker.API.Controllers
{
    [ApiController]
    [Route("api/albums")]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumService _albumService;

        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        [HttpGet("top10")]
        public async Task<IActionResult> GetTop10AlbumsFromDistributor(Guid distributorId)
        {
            var albums = await _albumService.GetAlbumsByDistributorId(distributorId);

            var topAlbums = albums.OrderByDescending(album => album.ReleaseDate)
                .Take(10)
                .ToList();

            return Ok(topAlbums);
        }

        [HttpGet("albums")]
        public async Task<IActionResult> GetAlbumsFromDistributor(Guid distributorId)
        {
            var albums = await _albumService.GetAlbumsByDistributorId(distributorId);

            return Ok(albums);
        }

        [HttpGet("album")]
        public async Task<IActionResult> GetAlbumById(Guid id)
        {
            var albums = await _albumService.GetAlbumById(id);

            return Ok(albums);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFilteredAlbums([FromQuery] AlbumFilter albumFilter)
        {
            var albums = await _albumService.GetAlbumsByFilter(albumFilter);

            return Ok(albums);
        }
    }
}