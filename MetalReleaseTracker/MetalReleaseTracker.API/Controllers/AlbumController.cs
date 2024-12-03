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

        [HttpGet("{id}")]
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